using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Omu.ValueInjecter;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Settings;
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
        private readonly IApplicationState _applicationState;
        private readonly IAutomationService _automationService;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;

        [ImportingConstructor]
        public TicketService(IDepartmentService departmentService, IApplicationState applicationState, IAutomationService automationService,
            IUserService userService, ISettingService settingService, ICacheService cacheService, IAccountService accountService)
        {
            _departmentService = departmentService;
            _applicationState = applicationState;
            _automationService = automationService;
            _userService = userService;
            _settingService = settingService;
            _cacheService = cacheService;
            _accountService = accountService;

            ValidatorRegistry.RegisterConcurrencyValidator(new TicketConcurrencyValidator());
        }

        public decimal GetExchangeRate(Account account)
        {
            if (account.ForeignCurrencyId == 0) return 1;
            return _cacheService.GetForeignCurrencies().Single(x => x.Id == account.ForeignCurrencyId).ExchangeRate;
        }

        public void UpdateAccount(Ticket ticket, Account account)
        {
            Debug.Assert(ticket != null);
            if (account == Account.Null)
            {
                var template = Dao.Single<AccountTransactionType>(
                        x => x.Id == _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionType.Id);
                account = _cacheService.GetAccountById(template.DefaultTargetAccountId);
            }
            ticket.UpdateAccount(account, GetExchangeRate(account));
            _automationService.NotifyEvent(RuleEventNames.AccountSelectedForTicket,
                    new
                    {
                        Ticket = ticket,
                        AccountName = account.Name,
                    });
        }

        public void UpdateResource(Ticket ticket, int ResourceTypeId, int resourceId, string resourceName, int accountId, string resourceCustomData)
        {
            var currentResource = ticket.TicketResources.SingleOrDefault(x => x.ResourceTypeId == ResourceTypeId);
            var currentResourceId = currentResource != null ? currentResource.ResourceId : 0;
            var newResourceName = resourceName;
            var oldResourceName = currentResource != null ? currentResource.ResourceName : "";
            if (currentResource != null && currentResource.ResourceId != resourceId)
            {
                var ResourceType = _cacheService.GetResourceTypeById(currentResource.ResourceTypeId);
                _automationService.NotifyEvent(RuleEventNames.ResourceUpdated, new
                {
                    currentResource.ResourceTypeId,
                    currentResource.ResourceId,
                    ResourceTypeName = ResourceType.Name,
                    OpenTicketCount = GetOpenTicketIds(currentResource.ResourceId).Count() - (ticket.Id > 0 ? 1 : 0)
                });
            }

            ticket.UpdateResource(ResourceTypeId, resourceId, resourceName, accountId, resourceCustomData);

            if (currentResourceId != resourceId)
            {
                _automationService.NotifyEvent(RuleEventNames.TicketResourceChanged,
                    new
                    {
                        Ticket = ticket,
                        ResourceTypeId = ResourceTypeId,
                        ResourceId = resourceId,
                        OldResourceName = oldResourceName,
                        NewResourceName = newResourceName,
                        OrderCount = ticket.Orders.Count
                    });
            }
        }

        public void UpdateResource(Ticket ticket, Resource resource)
        {
            if (resource == null) return;
            UpdateResource(ticket, resource.ResourceTypeId, resource.Id, resource.Name, resource.AccountId, resource.CustomData);
        }

        public Ticket OpenTicket(int ticketId)
        {
            Debug.Assert(_applicationState.CurrentDepartment != null);

            var ticket = ticketId == 0
                             ? CreateTicket()
                             : Dao.Load<Ticket>(ticketId,
                             x => x.Orders.Select(y => y.OrderTagValues),
                             x => x.Orders.Select(y => y.ProductTimerValue),
                             x => x.TicketResources,
                             x => x.Calculations,
                             x => x.Payments,
                             x => x.ChangePayments);

            _automationService.NotifyEvent(RuleEventNames.TicketOpened, new { Ticket = ticket, OrderCount = ticket.Orders.Count });

            return ticket;
        }

        private Ticket CreateTicket()
        {
            var account = _cacheService.GetAccountById(_applicationState.CurrentDepartment.TicketTemplate.SaleTransactionType.DefaultTargetAccountId);

            return Ticket.Create(_applicationState.CurrentDepartment.Model, account, GetExchangeRate(account), _cacheService.GetCalculationSelectors().Where(x => string.IsNullOrEmpty(x.ButtonHeader)).SelectMany(y => y.CalculationTypes));
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
                RecalculateTicket(ticket);
                ticket.UpdateIsClosed();

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
                    _automationService.NotifyEvent(RuleEventNames.TicketClosing, new { Ticket = ticket, TicketId = ticket.Id, NewOrderCount = ticket.GetUnlockedOrders().Count() });
                    ticket.LockTicket();
                }

                if (ticket.IsClosed)
                    ticket.TransactionDocument.AccountTransactions.Where(x => x.Amount == 0).ToList().ForEach(x => ticket.TransactionDocument.AccountTransactions.Remove(x));

                if (ticket.Id > 0)// eğer adisyonda satır yoksa ID burada 0 olmalı.
                    Dao.Save(ticket);


                Debug.Assert(ticket.Orders.Count(x => x.OrderNumber == 0) == 0);
            }

            if (ticket.Id > 0)
            {
                foreach (var ticketResource in ticket.TicketResources)
                {
                    var ResourceType = _cacheService.GetResourceTypeById(ticketResource.ResourceTypeId);
                    _automationService.NotifyEvent(RuleEventNames.ResourceUpdated, new
                                                                                       {
                                                                                           ticketResource.ResourceTypeId,
                                                                                           ticketResource.ResourceId,
                                                                                           ResourceTypeName = ResourceType.Name,
                                                                                           OpenTicketCount = GetOpenTicketIds(ticketResource.ResourceId).Count()
                                                                                       });
                }
            }

            result.TicketId = ticket.Id;
            return result;
        }

        public void AddPayment(Ticket ticket, PaymentType PaymentType, Account account, decimal tenderedAmount)
        {
            if (account == null) return;
            var remainingAmount = ticket.GetRemainingAmount();
            var changeAmount = tenderedAmount > remainingAmount ? tenderedAmount - remainingAmount : 0;
            ticket.AddPayment(PaymentType, account, tenderedAmount, GetExchangeRate(account), _applicationState.CurrentLoggedInUser.Id);
            _automationService.NotifyEvent(RuleEventNames.PaymentProcessed,
                new
                {
                    Ticket = ticket,
                    PaymentTypeName = PaymentType.Name,
                    Tenderedamount = tenderedAmount,
                    ProcessedAmount = tenderedAmount - changeAmount,
                    ChangeAmount = changeAmount,
                    RemainingAmount = ticket.GetRemainingAmount()
                });
        }

        public void AddChangePayment(Ticket ticket, ChangePaymentType PaymentType, Account account, decimal amount)
        {
            if (account == null) return;
            ticket.AddChangePayment(PaymentType, account, amount, GetExchangeRate(account), _applicationState.CurrentLoggedInUser.Id);
        }

        public void PayTicket(Ticket ticket, PaymentType template)
        {
            AddPayment(ticket, template, template.Account, ticket.GetRemainingAmount());
        }

        public void UpdateTicketNumber(Ticket ticket, Numerator numerator)
        {
            if (string.IsNullOrEmpty(ticket.TicketNumber))
            {
                ticket.TicketNumber = _settingService.GetNextString(numerator.Id);
            }
        }

        public TicketCommitResult MergeTickets(IEnumerable<int> ticketIds)
        {
            var ticketList = ticketIds.Select(OpenTicket).ToList();

            if (ticketList.Any(x => x.Calculations.Count() > 0))
                return new TicketCommitResult { ErrorMessage = string.Format("Can't merge tickets\r{0}", "contains calculations") };

            var resourcesUnMatches = ticketList.SelectMany(x => x.TicketResources).GroupBy(x => x.ResourceTypeId).Any(x => x.Select(y => y.ResourceId).Distinct().Count() > 1);
            if (resourcesUnMatches) return new TicketCommitResult { ErrorMessage = string.Format("Can't merge tickets\r{0}", "Resources doesn't match") };

            var clonedOrders = ticketList.SelectMany(x => x.Orders).Select(ObjectCloner.Clone).ToList();
            var clonedPayments = ticketList.SelectMany(x => x.Payments).Select(ObjectCloner.Clone2).ToList();
            var clonedChangePayments = ticketList.SelectMany(x => x.ChangePayments).Select(ObjectCloner.Clone2).ToList();
            var clonedTags = ticketList.SelectMany(x => x.GetTicketTagValues()).Select(ObjectCloner.Clone).ToList();
            var clonedResources = ticketList.SelectMany(x => x.TicketResources).Select(ObjectCloner.Clone).ToList();

            ticketList.ForEach(x => x.Orders.ToList().ForEach(x.RemoveOrder));
            ticketList.ForEach(x => x.Payments.ToList().ForEach(x.RemovePayment));
            ticketList.ForEach(x => x.ChangePayments.ToList().ForEach(x.RemoveChangePayment));
            ticketList.ForEach(x => x.Calculations.ToList().ForEach(x.RemoveCalculation));

            ticketList.ForEach(x => CloseTicket(x));

            var ticket = OpenTicket(0);
            clonedOrders.ForEach(ticket.Orders.Add);
            foreach (var cp in clonedPayments)
            {
                var account = _accountService.GetAccountById(cp.AccountTransaction.TargetTransactionValue.AccountId);
                ticket.AddPayment(_cacheService.GetPaymentTypeById(cp.PaymentTypeId), account, cp.Amount, GetExchangeRate(account), 0);
            }
            foreach (var cp in clonedChangePayments)
            {
                var account = _accountService.GetAccountById(cp.AccountTransaction.TargetTransactionValue.AccountId);
                ticket.AddChangePayment(_cacheService.GetChangePaymentTypeById(cp.ChangePaymentTypeId), account, cp.Amount, GetExchangeRate(account), 0);
            }

            clonedResources.ForEach(x => ticket.UpdateResource(x.ResourceTypeId, x.ResourceId, x.ResourceName, x.AccountId, x.ResourceCustomData));
            clonedTags.ForEach(x => ticket.SetTagValue(x.TagName, x.TagValue));

            foreach (var template in from order in clonedOrders.GroupBy(x => x.AccountTransactionTypeId)
                                     where !ticket.TransactionDocument.AccountTransactions.Any(x => x.AccountTransactionTypeId == order.Key)
                                     select _cacheService.GetAccountTransactionTypeById(order.Key))
            {
                ticket.TransactionDocument.AddNewTransaction(template, ticket.AccountTypeId, ticket.AccountId);
            }

            var result = CloseTicket(ticket);
            _automationService.NotifyEvent(RuleEventNames.TicketsMerged, new { Ticket = ticket });
            return result;
        }

        public TicketCommitResult MoveOrders(Ticket ticket, Order[] selectedOrders, int targetTicketId)
        {
            var clonedOrders = selectedOrders.Select(ObjectCloner.Clone2).ToList();
            ticket.RemoveOrders(selectedOrders);

            CloseTicket(ticket);
            ticket = OpenTicket(targetTicketId);

            foreach (var clonedOrder in clonedOrders)
            {
                clonedOrder.TicketId = 0;
                ticket.Orders.Add(clonedOrder);
            }

            foreach (var template in from order in clonedOrders.GroupBy(x => x.AccountTransactionTypeId)
                                     where !ticket.TransactionDocument.AccountTransactions.Any(x => x.AccountTransactionTypeId == order.Key)
                                     select _cacheService.GetAccountTransactionTypeById(order.Key))
            {
                ticket.TransactionDocument.AddNewTransaction(template, ticket.AccountTypeId, ticket.AccountId);
            }

            ticket.LastOrderDate = DateTime.Now;
            return CloseTicket(ticket);
        }

        public void RecalculateTicket(Ticket ticket)
        {
            var total = ticket.TotalAmount;
            ticket.Recalculate();
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

            tagData.PublishEvent(EventTopicNames.TicketTagSelected);
        }

        public int GetOpenTicketCount()
        {
            return Dao.Count<Ticket>(x => !x.IsClosed);
        }

        public IEnumerable<int> GetOpenTicketIds(int resourceId)
        {
            return Dao.Select<Ticket, int>(x => x.Id, x => !x.IsClosed && x.TicketResources.Any(y => y.ResourceId == resourceId));
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(int resourceId)
        {
            return GetOpenTickets(x => !x.IsClosed && x.TicketResources.Any(y => y.ResourceId == resourceId));
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction)
        {
            return Dao.Select(x => new OpenTicketData
            {
                Id = x.Id,
                LastOrderDate = x.LastOrderDate,
                TicketNumber = x.TicketNumber,
                RemainingAmount = x.RemainingAmount,
                Date = x.Date,
                TicketResources = x.TicketResources,
                TicketTags = x.TicketTags
            }, prediction, x => x.TicketResources);
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
                _cacheService.ResetTicketTagCache();
            }
        }

        public IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters)
        {
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);
            Expression<Func<Ticket, bool>> qFilter = x => x.Date >= startDate && x.Date < endDate;
            qFilter = filters.Aggregate(qFilter, (current, filter) => current.And(filter.GetExpression()));
            return Dao.Query(qFilter, x => x.TicketResources);
        }

        public IList<ITicketExplorerFilter> CreateTicketExplorerFilters()
        {
            var item = new TicketExplorerFilter(_cacheService) { FilterType = Resources.OnlyOpenTickets };
            return new List<ITicketExplorerFilter> { item };
        }

        public void UpdateAccountOfOpenTickets(Resource resource)
        {
            var openTicketDataList = GetOpenTickets(resource.Id);
            using (var w = WorkspaceFactory.Create())
            {
                foreach (var ticket in openTicketDataList.Select(data => w.Single<Ticket>(x => x.Id == data.Id, x => x.TicketResources)))
                {
                    ticket.TicketResources.Where(x => x.ResourceId == resource.Id).ToList().ForEach(x => x.AccountId = resource.AccountId);
                    w.CommitChanges();
                }
            }
        }

        public IEnumerable<Order> GetOrders(int id)
        {
            return Dao.Query<Order>(x => x.TicketId == id);
        }

        public void TagOrders(IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag)
        {
            foreach (var selectedOrder in selectedOrders)
            {
                var result = selectedOrder.ToggleOrderTag(orderTagGroup, orderTag, _applicationState.CurrentLoggedInUser.Id);
                if (orderTagGroup.SaveFreeTags && !orderTagGroup.OrderTags.Any(x => x.Name == orderTag.Name))
                {
                    using (var v = WorkspaceFactory.Create())
                    {
                        var og = v.Single<OrderTagGroup>(x => x.Id == orderTagGroup.Id);
                        if (og != null)
                        {
                            var lvTagName = orderTag.Name.ToLower();
                            var t = v.Single<OrderTag>(x => x.Name.ToLower() == lvTagName);
                            if (t == null)
                            {
                                var ot = new OrderTag();
                                ot.InjectFrom<CloneInjection>(orderTag);
                                og.OrderTags.Add(ot);
                                v.CommitChanges();
                                _cacheService.ResetOrderTagCache();
                            }
                        }
                    }
                }
                _automationService.NotifyEvent(result ? RuleEventNames.OrderTagged : RuleEventNames.OrderUntagged,
                new
                {
                    Order = selectedOrder,
                    OrderTagName = orderTagGroup.Name,
                    OrderTagValue = orderTag.Name
                });
            }
        }

        public void UntagOrders(IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag)
        {
            foreach (var selectedOrder in selectedOrders)
            {
                selectedOrder.UntagIfTagged(orderTagGroup, orderTag);
                _automationService.NotifyEvent(RuleEventNames.OrderUntagged,
                new
                {
                    Order = selectedOrder,
                    OrderTagName = orderTagGroup.Name,
                    OrderTagValue = orderTag.Name
                });
            }
        }

        public bool CanDeselectOrders(IEnumerable<Order> selectedOrders)
        {
            return selectedOrders.All(CanDeselectOrder);
        }

        public bool CanDeselectOrder(Order order)
        {
            if (!order.DecreaseInventory) return true;
            var ots = _cacheService.GetOrderTagGroupsForItem(order.MenuItemId);
            if (order.Locked) ots = ots.Where(x => !string.IsNullOrEmpty(x.ButtonHeader));
            return ots.Where(x => x.MinSelectedItems > 0).All(orderTagGroup => order.OrderTagValues.Count(x => x.OrderTagGroupId == orderTagGroup.Id) >= orderTagGroup.MinSelectedItems);
        }

        public OrderTagGroup GetMandantoryOrderTagGroup(Order order)
        {
            var ots = _cacheService.GetOrderTagGroupsForItem(order.MenuItemId);
            if (order.Locked) ots = ots.Where(x => !string.IsNullOrEmpty(x.ButtonHeader));
            return ots.Where(x => x.MinSelectedItems > 0).FirstOrDefault(orderTagGroup => order.OrderTagValues.Count(x => x.OrderTagGroupId == orderTagGroup.Id) < orderTagGroup.MinSelectedItems);
        }

        public bool CanCloseTicket(Ticket ticket)
        {
            if (!ticket.Locked)
                return CanDeselectOrders(ticket.Orders);
            return true;
        }

        public void RefreshAccountTransactions(Ticket ticket)
        {
            foreach (var template in from order in ticket.Orders.GroupBy(x => x.AccountTransactionTypeId)
                                     where !ticket.TransactionDocument.AccountTransactions.Any(x => x.AccountTransactionTypeId == order.Key)
                                     select _cacheService.GetAccountTransactionTypeById(order.Key))
            {
                var transaction = ticket.TransactionDocument.AddNewTransaction(template, ticket.AccountTypeId, ticket.AccountId);
                transaction.Reversable = false;
            }
        }

        public void UpdateOrderStates(Ticket selectedTicket, IEnumerable<Order> selectedOrders, OrderStateGroup orderStateGroup, OrderState orderState)
        {
            var so = selectedOrders.ToList();
            var accountTransactionTypeIds = so.GroupBy(x => x.AccountTransactionTypeId).Select(x => x.Key).ToList();
            so.ForEach(x => x.UpdateOrderState(orderStateGroup, orderState, _applicationState.CurrentLoggedInUser.Id));
            accountTransactionTypeIds.Where(x => !so.Any(y => y.AccountTransactionTypeId == x)).ToList()
                .ForEach(x => selectedTicket.TransactionDocument.AccountTransactions.Where(y => y.AccountTransactionTypeId == x).ToList().ForEach(y => selectedTicket.TransactionDocument.AccountTransactions.Remove(y)));
            RefreshAccountTransactions(selectedTicket);
        }

        public Order AddOrder(Ticket ticket, int menuItemId, decimal quantity, string portionName, OrderTagTemplate template)
        {
            if (ticket.Locked && !_userService.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets)) return null;
            if (!ticket.CanSubmit) return null;
            var menuItem = _cacheService.GetMenuItem(x => x.Id == menuItemId);
            var portion = _cacheService.GetMenuItemPortion(menuItemId, portionName);
            if (portion == null) return null;
            var priceTag = _applicationState.CurrentDepartment.PriceTag;
            var productTimer = _cacheService.GetProductTimer(menuItemId);
            var order = ticket.AddOrder(
                _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionType,
                _applicationState.CurrentLoggedInUser.Name, menuItem, portion, priceTag, productTimer);

            order.Quantity = quantity > 9 ? decimal.Round(quantity / portion.Multiplier, 3, MidpointRounding.AwayFromZero) : quantity;

            if (template != null) template.OrderTagTemplateValues.ToList().ForEach(x => order.ToggleOrderTag(x.OrderTagGroup, x.OrderTag, 0));
            RecalculateTicket(ticket);

            order.PublishEvent(EventTopicNames.OrderAdded);
            _automationService.NotifyEvent(RuleEventNames.TicketLineAdded, new { Ticket = ticket, Order = order, order.MenuItemName });

            return order;
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
                //todo fix : Check Resources instead
                if (current.AccountName != loaded.AccountName)
                {
                    return ConcurrencyCheckResult.Break(string.Format(Resources.TicketMovedRetryLastOperation_f, loaded.AccountName));
                }

                if (current.IsClosed != loaded.IsClosed)
                {
                    if (loaded.IsClosed)
                    {
                        return ConcurrencyCheckResult.Break(Resources.TicketPaidChangesNotSaved);
                    }
                    if (current.IsClosed)
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
}
