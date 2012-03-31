using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Data.Specification;
using Samba.Services.Common;

namespace Samba.Services.Implementations.TicketModule
{
    [Export(typeof(ITicketService))]
    public class TicketService : AbstractService, ITicketService
    {
        private readonly IDepartmentService _departmentService;
        private readonly IPrinterService _printerService;
        private readonly IApplicationState _applicationState;
        private readonly IAutomationService _automationService;
        private readonly ISettingService _settingService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public TicketService(IDepartmentService departmentService, IPrinterService printerService,
            IApplicationState applicationState, IAutomationService automationService,
            ISettingService settingService, ICacheService cacheService)
        {
            _departmentService = departmentService;
            _printerService = printerService;
            _applicationState = applicationState;
            _automationService = automationService;
            _settingService = settingService;
            _cacheService = cacheService;

            ValidatorRegistry.RegisterDeleteValidator(new TicketTagGroupDeleteValidator());
            ValidatorRegistry.RegisterConcurrencyValidator(new TicketConcurrencyValidator());
        }

        public void UpdateAccount(Ticket ticket, Account account)
        {
            Debug.Assert(ticket != null);
            if (account == Account.Null)
            {
                var template = Dao.Single<AccountTransactionTemplate>(
                        x => x.Id == _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate.Id);
                account = _cacheService.GetAccountById(template.DefaultTargetAccountId);
            }
            ticket.UpdateAccount(account);
            _automationService.NotifyEvent(RuleEventNames.AccountSelectedForTicket,
                    new
                    {
                        Ticket = ticket,
                        AccountName = account.Name,
                    });
        }

        public void UpdateResource(Ticket ticket, Resource resource)
        {
            Debug.Assert(ticket != null);
            ticket.UpdateResource(resource);
        }

        public Ticket OpenTicket(int ticketId)
        {
            Debug.Assert(_applicationState.CurrentDepartment != null);

            var ticket = ticketId == 0
                             ? CreateTicket()
                             : Dao.Load<Ticket>(ticketId, x => x.TicketResources,
                             x => x.Orders.Select(y => y.OrderTagValues), x => x.Calculations,
                             x => x.Payments, x => x.PaidItems, x => x.Tags);

            if (ticket.Id == 0)
                _automationService.NotifyEvent(RuleEventNames.TicketCreated, new { Ticket = ticket });

            return ticket;
        }

        private Ticket CreateTicket()
        {
            var account = _cacheService.GetAccountById(_applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate.DefaultTargetAccountId);
            return Ticket.Create(_applicationState.CurrentDepartment, account);
        }

        public TicketCommitResult CloseTicket(Ticket ticket)
        {
            var result = new TicketCommitResult();
            Debug.Assert(ticket != null);

            result.ErrorMessage = Dao.CheckConcurrency(ticket);
            var changed = !string.IsNullOrEmpty(result.ErrorMessage);
            var canSumbitTicket = !changed && ticket.CanSubmit; // Fişi kaydedebilmek için gün sonu yapılmamış ve fişin ödenmemiş olması gerekir.

            if (canSumbitTicket)
            {
                ticket.Recalculate(_settingService.ProgramSettings.AutoRoundDiscount, _applicationState.CurrentLoggedInUser.Id);
                ticket.IsPaid = ticket.RemainingAmount == 0;

                if (ticket.Orders.Count > 0)
                {
                    var department = _departmentService.GetDepartment(ticket.DepartmentId);

                    if (ticket.Orders.Where(x => !x.Locked).FirstOrDefault() != null)
                    {
                        var number = _settingService.GetNextNumber(department.TicketTemplate.OrderNumerator.Id);
                        ticket.MergeOrdersAndUpdateOrderNumbers(number);
                        ticket.Orders.Where(x => x.Id == 0).ToList().ForEach(x => x.CreatedDateTime = DateTime.Now);
                    }

                    if (ticket.Id == 0)
                    {
                        UpdateTicketNumber(ticket, department.TicketTemplate.TicketNumerator);
                        ticket.LastOrderDate = DateTime.Now;
                        Dao.Save(ticket);
                    }

                    Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
                    Debug.Assert(ticket.Id > 0);

                    //Otomatik yazdırma
                    _printerService.AutoPrintTicket(ticket);
                    ticket.LockTicket();
                }

                if (ticket.Id > 0)// eğer adisyonda satır yoksa ID burada 0 olmalı.
                    Dao.Save(ticket);
                Debug.Assert(ticket.Orders.Count(x => x.OrderNumber == 0) == 0);
            }
            if (!changed)
                _automationService.NotifyEvent(RuleEventNames.TicketClosed, new { Ticket = ticket, ticket.AccountId, ticket.RemainingAmount });
            result.TicketId = ticket.Id;
            return result;
        }

        public void AddPayment(Ticket ticket, PaymentTemplate template, decimal tenderedAmount)
        {
            ticket.AddPayment(template, tenderedAmount, _applicationState.CurrentLoggedInUser.Id);
        }

        public void PaySelectedTicket(Ticket ticket, PaymentTemplate template)
        {
            AddPayment(ticket, template, ticket.GetRemainingAmount());
        }

        public void UpdateTicketNumber(Ticket ticket, Numerator numerator)
        {
            if (string.IsNullOrEmpty(ticket.TicketNumber))
            {
                ticket.TicketNumber = _settingService.GetNextString(numerator.Id);
            }
        }

        public TicketCommitResult MoveOrders(Ticket ticket, IEnumerable<Order> selectedOrders, int targetTicketId)
        {
            var clonedOrders = selectedOrders.Select(ObjectCloner.Clone).ToList();
            ticket.RemoveOrders(selectedOrders);

            CloseTicket(ticket);
            ticket = OpenTicket(targetTicketId);

            foreach (var clonedOrder in clonedOrders)
            {
                clonedOrder.TicketId = 0;
                ticket.Orders.Add(clonedOrder);
            }

            foreach (var template in from order in clonedOrders.GroupBy(x => x.AccountTransactionTemplateId)
                                     where !ticket.AccountTransactions.AccountTransactions.Any(x => x.AccountTransactionTemplateId == order.Key)
                                     select _cacheService.GetAccountTransactionTemplateById(order.Key))
            {
                ticket.AccountTransactions.AccountTransactions.Add(AccountTransaction.Create(template));
            }

            ticket.LastOrderDate = DateTime.Now;
            return CloseTicket(ticket);
        }

        public void RecalculateTicket(Ticket ticket)
        {
            var total = ticket.TotalAmount;
            ticket.Recalculate(_settingService.ProgramSettings.AutoRoundDiscount, _applicationState.CurrentLoggedInUser.Id);
            if (total != ticket.TotalAmount)
            {
                _automationService.NotifyEvent(RuleEventNames.TicketTotalChanged,
                    new
                    {
                        Ticket = ticket,
                        PreviousTotal = total,
                        TicketTotal = ticket.GetSum(),
                        DiscountTotal = ticket.GetPreTaxServicesTotal(),
                        PaymentTotal = ticket.GetPaymentAmount()
                    });
            }
        }

        public void RegenerateTaxRates(Ticket ticket)
        {
            foreach (var order in ticket.Orders)
            {
                var order1 = order;
                var mi = _cacheService.GetMenuItem(x => x.Id == order1.MenuItemId);
                if (mi == null) continue;
                var item = order1;
                var portion = mi.Portions.FirstOrDefault(x => x.Name == item.PortionName);
                if (portion != null) order1.UpdatePortion(portion, order1.PriceTag, mi.TaxTemplate);
            }
        }

        public void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag)
        {
            ticket.SetTagValue(tagGroup.Name, ticketTag.Name);
            if (tagGroup.Numerator != null)
            {
                ticket.TicketNumber = "";
                UpdateTicketNumber(ticket, tagGroup.Numerator);
            }

            if (ticketTag.AccountId > 0)
                UpdateAccount(ticket, Dao.SingleWithCache<Account>(x => x.Id == ticketTag.AccountId));

            if (tagGroup.SaveFreeTags)
            {
                SaveFreeTicketTag(tagGroup.Id, ticketTag.Name);
            }

            var tagData = new TicketTagData
            {
                Ticket = ticket,
                TicketTagGroup = tagGroup,
                TagName = tagGroup.Name,
                TagValue = ticketTag.Name
            };

            _automationService.NotifyEvent(RuleEventNames.TicketTagSelected,
                        new
                        {
                            Ticket = ticket,
                            tagData.TagName,
                            tagData.TagValue,
                            NumericValue = tagGroup.IsNumeric ? Convert.ToDecimal(ticketTag.Name) : 0,
                            TicketTag = ticket.GetTagData()
                        });

            tagData.PublishEvent(EventTopicNames.TagSelectedForSelectedTicket);
        }

        public int GetOpenTicketCount()
        {
            return Dao.Count<Ticket>(x => !x.IsPaid);
        }

        public IEnumerable<int> GetOpenTickets(int accountId)
        {
            return Dao.Select<Ticket, int>(x => x.Id, x => x.RemainingAmount > 0 && x.AccountId == accountId);
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction)
        {
            return Dao.Select(x => new OpenTicketData
            {
                Id = x.Id,
                LastOrderDate = x.LastOrderDate,
                TicketNumber = x.TicketNumber,
                AccountName = x.AccountName,
                RemainingAmount = x.RemainingAmount,
                Date = x.Date
            }, prediction);
        }

        public void SaveFreeTicketTag(int id, string freeTag)
        {
            if (string.IsNullOrEmpty(freeTag)) return;

            using (var workspace = WorkspaceFactory.Create())
            {
                var tt = workspace.Single<TicketTagGroup>(x => x.Id == id);
                Debug.Assert(tt != null);
                var tag = tt.TicketTags.SingleOrDefault(x => x.Name.ToLower() == freeTag.ToLower());

                if (tag != null) return;
                tag = new TicketTag { Name = freeTag };
                tt.TicketTags.Add(tag);
                workspace.Add(tag);
                workspace.CommitChanges();
                Dao.ResetCache();
            }
        }

        public IList<TicketExplorerRowData> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters)
        {
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);
            Expression<Func<Ticket, bool>> qFilter = x => x.Date >= startDate && x.Date < endDate;
            qFilter = filters.Aggregate(qFilter, (current, filter) => current.And(filter.GetExpression()));
            return Dao.Query(qFilter).Select(x => new TicketExplorerRowData(x)).ToList();
        }

        public IList<ITicketExplorerFilter> CreateTicketExplorerFilters()
        {
            var item = new TicketExplorerFilter { FilterType = FilterType.OpenTickets };
            return new List<ITicketExplorerFilter> { item };
        }

        public IEnumerable<Order> ExtractSelectedOrders(Ticket model, IEnumerable<Order> selectedOrders)
        {
            var selectedItems = selectedOrders.Where(x => x.SelectedQuantity > 0 && x.SelectedQuantity < x.Quantity).ToList();
            var newItems = model.ExtractSelectedOrders(selectedItems);
            return newItems;
        }

        public override void Reset()
        {

        }
    }

    public class TicketConcurrencyValidator : ConcurrencyValidator<Ticket>
    {
        public override ConcurrencyCheckResult GetErrorMessage(Ticket current, Ticket loaded)
        {
            if (current.Id > 0)
            {
                if (current.AccountName != loaded.AccountName)
                {
                    return ConcurrencyCheckResult.Break(string.Format(Resources.TicketMovedRetryLastOperation_f, loaded.AccountName));
                }

                if (current.IsPaid != loaded.IsPaid)
                {
                    if (loaded.IsPaid)
                    {
                        return ConcurrencyCheckResult.Break(Resources.TicketPaidChangesNotSaved);
                    }
                    if (current.IsPaid)
                    {
                        return ConcurrencyCheckResult.Break(Resources.TicketChangedRetryLastOperation);
                    }
                }
                else if (current.LastPaymentDate != loaded.LastPaymentDate)
                {
                    var currentPaymentIds = current.Payments.Select(x => x.Id).Distinct();
                    var unknownPayments = loaded.Payments.Where(x => !currentPaymentIds.Contains(x.Id)).FirstOrDefault();
                    if (unknownPayments != null)
                    {
                        return ConcurrencyCheckResult.Break(Resources.TicketPaidLastChangesNotSaved);
                    }
                }

                if (current.RemainingAmount == 0 && loaded.GetSum() != current.GetSum())
                {
                    return ConcurrencyCheckResult.Break(Resources.TicketChangedRetryLastOperation);
                }
            }

            return ConcurrencyCheckResult.Continue();
        }
    }

    public class TicketTagGroupDeleteValidator : SpecificationValidator<TicketTagGroup>
    {
        public override string GetErrorMessage(TicketTagGroup model)
        {
            if (Dao.Exists<TicketTemplate>(x => x.TicketTagGroups.Any(y => y.Id == model.Id)))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.TicketTagGroup, Resources.TicketTemplate);
            return "";
        }
    }
}
