using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule.ServiceImplementations
{
    [Export(typeof(ITicketService))]
    public class TicketService : AbstractService, ITicketService
    {
        private IWorkspace _workspace;
        private readonly IDepartmentService _departmentService;
        private readonly IPrinterService _printerService;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;

        [ImportingConstructor]
        public TicketService(IDepartmentService departmentService, IPrinterService printerService,
            IApplicationState applicationState, IApplicationStateSetter applicationStateSetter)
        {
            _departmentService = departmentService;
            _printerService = printerService;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
        }

        public void UpdateAccount(Ticket ticket, Account account)
        {
            Debug.Assert(ticket != null);
            ticket.UpdateAccount(CheckAccount(account));
        }

        public Ticket OpenTicket(int ticketId)
        {
            var ticket = _applicationState.CurrentTicket;
            Debug.Assert(_workspace == null);
            Debug.Assert(ticket == null);
            Debug.Assert(_applicationState.CurrentDepartment != null);

            _workspace = WorkspaceFactory.Create();
            ticket = ticketId == 0
                                 ? Ticket.Create(_applicationState.CurrentDepartment)
                                 : _workspace.Single<Ticket>(t => t.Id == ticketId,
                                                             x => x.Orders.Select(y => y.OrderTagValues));
            _applicationStateSetter.SetCurrentTicket(ticket);

            if (ticket.Id == 0)
                RuleExecutor.NotifyEvent(RuleEventNames.TicketCreated, new { Ticket = ticket });

            return ticket;
        }

        public Ticket OpenTicketByLocationName(string locationName)
        {
            var location = Dao.SingleWithCache<Location>(x => x.Name == locationName);
            if (location != null)
            {
                var ticket = _applicationState.CurrentTicket;
                if (location.TicketId > 0)
                    ticket = OpenTicket(location.TicketId);
                ChangeTicketLocation(ticket, location.Id);
                return ticket;
            }
            return null;
        }

        public Ticket OpenTicketByTicketNumber(string ticketNumber)
        {
            Debug.Assert(_applicationState.CurrentTicket == null);
            var id = Dao.Select<Ticket, int>(x => x.Id, x => x.TicketNumber == ticketNumber).FirstOrDefault();
            if (id > 0)
            {
                var ticket = OpenTicket(id);
                return ticket;
            }
            return null;
        }

        public TicketCommitResult CloseTicket(Ticket ticket)
        {
            var department = _departmentService.GetDepartment(ticket.DepartmentId);
            var result = new TicketCommitResult();
            Debug.Assert(ticket != null);
            var changed = false;

            if (ticket.Id > 0)
            {
                var lup = Dao.Single<Ticket, DateTime>(ticket.Id, x => x.LastUpdateTime);
                if (ticket.LastUpdateTime.CompareTo(lup) != 0)
                {
                    var currentTicket = Dao.Single<Ticket>(x => x.Id == ticket.Id, x => x.Orders, x => x.Payments);
                    if (currentTicket.LocationName != ticket.LocationName)
                    {
                        result.ErrorMessage = string.Format(Resources.TicketMovedRetryLastOperation_f, currentTicket.LocationName);
                        changed = true;
                    }

                    if (currentTicket.IsPaid != ticket.IsPaid)
                    {
                        if (currentTicket.IsPaid)
                        {
                            result.ErrorMessage = Resources.TicketPaidChangesNotSaved;
                        }
                        if (ticket.IsPaid)
                        {
                            result.ErrorMessage = Resources.TicketChangedRetryLastOperation;
                        }
                        changed = true;
                    }
                    else if (currentTicket.LastPaymentDate != ticket.LastPaymentDate)
                    {
                        var currentPaymentIds = ticket.Payments.Select(x => x.Id).Distinct();
                        var unknownPayments = currentTicket.Payments.Where(x => !currentPaymentIds.Contains(x.Id)).FirstOrDefault();
                        if (unknownPayments != null)
                        {
                            result.ErrorMessage = Resources.TicketPaidLastChangesNotSaved;
                            changed = true;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(ticket.LocationName) && ticket.Id == 0)
            {
                var ticketId = Dao.Select<Location, int>(x => x.TicketId, x => x.Name == ticket.LocationName).FirstOrDefault();
                {
                    if (ticketId > 0)
                    {
                        result.ErrorMessage = string.Format(Resources.LocationChangedRetryLastOperation_f, ticket.LocationName);
                        changed = true;
                    }
                }
            }

            var canSumbitTicket = !changed && ticket.CanSubmit; // Fişi kaydedebilmek için gün sonu yapılmamış ve fişin ödenmemiş olması gerekir.

            if (canSumbitTicket)
            {
                ticket.Recalculate(AppServices.SettingService.AutoRoundDiscount, _applicationState.CurrentLoggedInUser.Id);
                ticket.IsPaid = ticket.RemainingAmount == 0;

                if (ticket.Orders.Count > 0)
                {
                    if (ticket.Orders.Where(x => !x.Locked).FirstOrDefault() != null)
                    {
                        ticket.MergeOrdersAndUpdateOrderNumbers(NumberGenerator.GetNextNumber(department.TicketTemplate.OrderNumerator.Id));
                        ticket.Orders.Where(x => x.Id == 0).ToList().ForEach(x => x.CreatedDateTime = DateTime.Now);
                    }

                    if (ticket.Id == 0)
                    {
                        _workspace.Add(ticket);
                        UpdateTicketNumber(ticket, department.TicketTemplate.TicketNumerator);
                        ticket.LastOrderDate = DateTime.Now;
                        _workspace.CommitChanges();
                    }

                    Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
                    Debug.Assert(ticket.Id > 0);

                    //Otomatik yazdırma
                    _printerService.AutoPrintTicket(ticket);
                    ticket.LockTicket();
                }

                UpdateTicketLocation(ticket);
                if (ticket.Id > 0)  // eğer adisyonda satır yoksa ID burada 0 olmalı.
                    _workspace.CommitChanges();
                Debug.Assert(ticket.Orders.Count(x => x.OrderNumber == 0) == 0);
            }
            result.TicketId = ticket.Id;
            _workspace = null;
            _applicationStateSetter.SetCurrentTicket(null);

            return result;
        }

        public void AddPayment(Ticket ticket, decimal tenderedAmount, DateTime date, PaymentType paymentType)
        {
            ticket.AddPayment(date, tenderedAmount, paymentType, _applicationState.CurrentLoggedInUser.Id);
        }

        public void PaySelectedTicket(Ticket ticket, PaymentType paymentType)
        {
            AddPayment(ticket, ticket.GetRemainingAmount(), DateTime.Now, paymentType);
        }

        public void UpdateTicketNumber(Ticket ticket, Numerator numerator)
        {
            if (string.IsNullOrEmpty(ticket.TicketNumber))
                ticket.TicketNumber = NumberGenerator.GetNextString(numerator.Id);
        }

        private void UpdateTicketLocation(Ticket ticket)
        {
            if (string.IsNullOrEmpty(ticket.LocationName)) return;
            var location = _workspace.Single<Location>(x => x.Name == ticket.LocationName);
            if (location != null)
            {
                if (ticket.IsPaid || ticket.Orders.Count == 0)
                {
                    if (location.TicketId == ticket.Id)
                    {
                        location.Reset();
                    }
                }
                else
                {
                    location.TicketId = ticket.Id;
                    location.IsTicketLocked = ticket.Locked;
                }
            }
            else ticket.LocationName = "";
        }

        public void ChangeTicketLocation(Ticket ticket, int locationId)
        {
            Debug.Assert(ticket != null);

            var location = _workspace.Single<Location>(x => x.Id == locationId);
            var oldLocation = "";

            if (!string.IsNullOrEmpty(ticket.LocationName))
            {
                oldLocation = ticket.LocationName;
                var oldLoc = _workspace.Single<Location>(x => x.Name == oldLocation);
                if (oldLoc.TicketId == ticket.Id)
                {
                    oldLoc.Reset();
                }
            }

            if (location.TicketId > 0 && location.TicketId != ticket.Id)
            {
                MoveOrders(ticket, ticket.Orders.ToList(), location.TicketId);
                ticket = OpenTicket(location.TicketId);
            }

            ticket.LocationName = location.Name;
            if (_applicationState.CurrentDepartment != null) ticket.DepartmentId = _applicationState.CurrentDepartment.Id;
            location.TicketId = ticket.GetRemainingAmount() > 0 ? ticket.Id : 0;

            RuleExecutor.NotifyEvent(RuleEventNames.TicketLocationChanged, new { Ticket = ticket, OldLocation = oldLocation, NewLocation = ticket.LocationName });
        }

        public Account CheckAccount(Account account)
        {
            if (account == Account.Null)
                return Account.Null;

            if (account.Id == 0)
            {
                using (var workspace = WorkspaceFactory.Create())
                {
                    workspace.Add(account);
                    workspace.CommitChanges();
                }
                return account;
            }

            var result = _workspace.Single<Account>(
                    x => x.Id == account.Id
                    && x.Name == account.Name
                    && x.SearchString == account.SearchString);

            if (result == null)
            {
                result = _workspace.Single<Account>(x => x.Id == account.Id);
                Debug.Assert(result != null);
                result.Name = account.Name;
                result.SearchString = account.SearchString;
            }
            return result;
        }

        public TicketCommitResult MoveOrders(Ticket ticket, IEnumerable<Order> selectedOrders, int targetTicketId)
        {
            var clonedOrders = selectedOrders.Select(ObjectCloner.Clone).ToList();
            ticket.RemoveOrders(selectedOrders);

            if (ticket.Orders.Count == 0)
            {
                var info = targetTicketId.ToString();
                if (targetTicketId > 0)
                {
                    var tData = Dao.Single<Ticket, dynamic>(targetTicketId, x => new { x.LocationName, x.TicketNumber });
                    info = tData.LocationName + " - " + tData.TicketNumber;
                }
                if (!string.IsNullOrEmpty(ticket.Note)) ticket.Note += "\r";
                ticket.Note += ticket.LocationName + " => " + info;
            }

            CloseTicket(ticket);
            ticket = OpenTicket(targetTicketId);

            foreach (var clonedOrder in clonedOrders)
            {
                ticket.Orders.Add(clonedOrder);
            }

            ticket.LastOrderDate = DateTime.Now;
            return CloseTicket(ticket);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(MenuItem menuItem)
        {
            return GetOrderTagGroupsForItem(_applicationState.CurrentDepartment.TicketTemplate.OrderTagGroups, menuItem);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<MenuItem> menuItems)
        {
            return menuItems.Aggregate(_applicationState.CurrentDepartment.TicketTemplate.OrderTagGroups.OrderBy(x => x.Order) as IEnumerable<OrderTagGroup>, GetOrderTagGroupsForItem);
        }

        private static IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(IEnumerable<OrderTagGroup> tagGroups, MenuItem menuItem)
        {
            var maps = tagGroups.SelectMany(x => x.OrderTagMaps)
                .Where(x => x.MenuItemGroupCode == menuItem.GroupCode || x.MenuItemGroupCode == null)
                .Where(x => x.MenuItemId == menuItem.Id || x.MenuItemId == 0);

            return tagGroups.Where(x => maps.Any(y => y.OrderTagGroupId == x.Id));
        }

        public void RecalculateTicket(Ticket ticket)
        {
            var total = ticket.TotalAmount;
            ticket.Recalculate(AppServices.SettingService.AutoRoundDiscount, _applicationState.CurrentLoggedInUser.Id);
            if (total != ticket.TotalAmount)
            {
                RuleExecutor.NotifyEvent(RuleEventNames.TicketTotalChanged,
                    new
                    {
                        Ticket = ticket,
                        PreviousTotal = total,
                        TicketTotal = ticket.GetSum(),
                        DiscountTotal = ticket.GetDiscountAndRoundingTotal(),
                        PaymentTotal = ticket.GetPaymentAmount()
                    });
            }
        }

        public void RegenerateTaxRates(Ticket ticket)
        {
            foreach (var order in ticket.Orders)
            {
                var mi = AppServices.DataAccessService.GetMenuItem(order.MenuItemId);
                if (mi == null) continue;
                var item = order;
                var portion = mi.Portions.FirstOrDefault(x => x.Name == item.PortionName);
                if (portion != null) order.UpdatePortion(portion, order.PriceTag, mi.TaxTemplate);
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

            var tagData = new TicketTagData { Action = tagGroup.Action, TagName = tagGroup.Name, TagValue = ticketTag.Name, NumericValue = tagGroup.IsNumeric ? Convert.ToDecimal(ticketTag.Name) : 0 };

            RuleExecutor.NotifyEvent(RuleEventNames.TicketTagSelected,
                        new
                        {
                            Ticket = ticket,
                            tagData.TagName,
                            tagData.TagValue,
                            tagData.NumericValue,
                            TicketTag = ticket.GetTagData()
                        });

            tagData.PublishEvent(EventTopicNames.TagSelectedForSelectedTicket);
        }

        public void ResetLocationData(Ticket ticket)
        {
            _workspace.All<Location>(x => x.TicketId == ticket.Id).ToList().ForEach(x => x.Reset());
            UpdateTicketLocation(ticket);
            Debug.Assert(_workspace != null);
            Debug.Assert(ticket != null);
            Debug.Assert(ticket.Id > 0 || ticket.Orders.Count > 0);
            if (ticket.Id == 0 && ticket.TicketNumber != null)
                _workspace.Add(ticket);
            ticket.LastUpdateTime = DateTime.Now;
            _workspace.CommitChanges();
        }

        public void AddItemToSelectedTicket(Order newItem)
        {
            _workspace.Add(newItem);
        }

        public override void Reset()
        {

        }
    }
}
