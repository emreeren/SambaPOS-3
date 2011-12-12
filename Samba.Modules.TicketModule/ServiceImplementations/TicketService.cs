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
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule.ServiceImplementations
{
    [Export(typeof(ITicketService))]
    public class TicketService : ITicketService
    {
        private IWorkspace _workspace;
        private readonly IDepartmentService _departmentService;

        [ImportingConstructor]
        public TicketService(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public void UpdateAccount(Ticket ticket, Account account)
        {
            Debug.Assert(ticket != null);
            ticket.UpdateAccount(CheckAccount(account));
        }

        public Ticket CurrentTicket { get; private set; }

        public void OpenTicket(int ticketId)
        {
            Debug.Assert(_workspace == null);
            Debug.Assert(CurrentTicket == null);
            Debug.Assert(_departmentService.CurrentDepartment != null);

            _workspace = WorkspaceFactory.Create();
            CurrentTicket = ticketId == 0
                                ? Ticket.Create(_departmentService.CurrentDepartment)
                                : _workspace.Single<Ticket>(ticket => ticket.Id == ticketId,
                                                            x => x.Orders.Select(y => y.OrderTagValues));
            if (CurrentTicket.Id == 0)
                RuleExecutor.NotifyEvent(RuleEventNames.TicketCreated, new { Ticket = CurrentTicket });

        }

        public void OpenTicketByLocationName(string locationName)
        {
            var location = Dao.SingleWithCache<Location>(x => x.Name == locationName);
            if (location != null)
            {
                if (location.TicketId > 0) OpenTicket(location.TicketId);
                UpdateLocation(location.Id);
            }
        }

        public void OpenTicketByTicketNumber(string ticketNumber)
        {
            Debug.Assert(CurrentTicket == null);
            var id = Dao.Select<Ticket, int>(x => x.Id, x => x.TicketNumber == ticketNumber).FirstOrDefault();
            if (id > 0) OpenTicket(id);
        }

        public TicketCommitResult CloseTicket()
        {
            var department = _departmentService.GetDepartment(CurrentTicket.DepartmentId);
            var result = new TicketCommitResult();
            Debug.Assert(CurrentTicket != null);
            var changed = false;

            if (CurrentTicket.Id > 0)
            {
                var lup = Dao.Single<Ticket, DateTime>(CurrentTicket.Id, x => x.LastUpdateTime);
                if (CurrentTicket.LastUpdateTime.CompareTo(lup) != 0)
                {
                    var currentTicket = Dao.Single<Ticket>(x => x.Id == CurrentTicket.Id, x => x.Orders, x => x.Payments);
                    if (currentTicket.LocationName != CurrentTicket.LocationName)
                    {
                        result.ErrorMessage = string.Format(Resources.TicketMovedRetryLastOperation_f, currentTicket.LocationName);
                        changed = true;
                    }

                    if (currentTicket.IsPaid != CurrentTicket.IsPaid)
                    {
                        if (currentTicket.IsPaid)
                        {
                            result.ErrorMessage = Resources.TicketPaidChangesNotSaved;
                        }
                        if (CurrentTicket.IsPaid)
                        {
                            result.ErrorMessage = Resources.TicketChangedRetryLastOperation;
                        }
                        changed = true;
                    }
                    else if (currentTicket.LastPaymentDate != CurrentTicket.LastPaymentDate)
                    {
                        var currentPaymentIds = CurrentTicket.Payments.Select(x => x.Id).Distinct();
                        var unknownPayments = currentTicket.Payments.Where(x => !currentPaymentIds.Contains(x.Id)).FirstOrDefault();
                        if (unknownPayments != null)
                        {
                            result.ErrorMessage = Resources.TicketPaidLastChangesNotSaved;
                            changed = true;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(CurrentTicket.LocationName) && CurrentTicket.Id == 0)
            {
                var ticketId = Dao.Select<Location, int>(x => x.TicketId, x => x.Name == CurrentTicket.LocationName).FirstOrDefault();
                {
                    if (ticketId > 0)
                    {
                        result.ErrorMessage = string.Format(Resources.LocationChangedRetryLastOperation_f, CurrentTicket.LocationName);
                        changed = true;
                    }
                }
            }

            var canSumbitTicket = !changed && CurrentTicket.CanSubmit; // Fişi kaydedebilmek için gün sonu yapılmamış ve fişin ödenmemiş olması gerekir.

            if (canSumbitTicket)
            {
                CurrentTicket.Recalculate(AppServices.SettingService.AutoRoundDiscount, AppServices.CurrentLoggedInUser.Id);
                CurrentTicket.IsPaid = CurrentTicket.RemainingAmount == 0;

                if (CurrentTicket.Orders.Count > 0)
                {
                    if (CurrentTicket.Orders.Where(x => !x.Locked).FirstOrDefault() != null)
                    {
                        CurrentTicket.MergeOrdersAndUpdateOrderNumbers(NumberGenerator.GetNextNumber(department.TicketTemplate.OrderNumerator.Id));
                        CurrentTicket.Orders.Where(x => x.Id == 0).ToList().ForEach(x => x.CreatedDateTime = DateTime.Now);
                    }

                    if (CurrentTicket.Id == 0)
                    {
                        _workspace.Add(CurrentTicket);
                        UpdateTicketNumber(CurrentTicket, department.TicketTemplate.TicketNumerator);
                        CurrentTicket.LastOrderDate = DateTime.Now;
                        _workspace.CommitChanges();
                    }

                    Debug.Assert(!string.IsNullOrEmpty(CurrentTicket.TicketNumber));
                    Debug.Assert(CurrentTicket.Id > 0);

                    //Otomatik yazdırma
                    AppServices.PrintService.AutoPrintTicket(CurrentTicket);
                    CurrentTicket.LockTicket();
                }

                UpdateTicketLocation(CurrentTicket);
                if (CurrentTicket.Id > 0)  // eğer adisyonda satır yoksa ID burada 0 olmalı.
                    _workspace.CommitChanges();
                Debug.Assert(CurrentTicket.Orders.Count(x => x.OrderNumber == 0) == 0);
            }
            result.TicketId = CurrentTicket.Id;
            _workspace = null;
            CurrentTicket = null;

            return result;
        }

        public void AddPayment(decimal tenderedAmount, DateTime date, PaymentType paymentType)
        {
            CurrentTicket.AddPayment(date, tenderedAmount, paymentType, AppServices.CurrentLoggedInUser.Id);
        }

        public void PaySelectedTicket(PaymentType paymentType)
        {
            AddPayment(CurrentTicket.GetRemainingAmount(), DateTime.Now, paymentType);
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
                        location.TicketId = 0;
                        location.IsTicketLocked = false;
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

        public void UpdateLocation(int locationId)
        {
            Debug.Assert(CurrentTicket != null);

            var location = _workspace.Single<Location>(x => x.Id == locationId);
            string oldLocation = "";

            if (!string.IsNullOrEmpty(CurrentTicket.LocationName))
            {
                oldLocation = CurrentTicket.LocationName;
                var oldLoc = _workspace.Single<Location>(x => x.Name == CurrentTicket.LocationName);
                if (oldLoc.TicketId == CurrentTicket.Id)
                {
                    oldLoc.IsTicketLocked = false;
                    oldLoc.TicketId = 0;
                }
            }

            if (location.TicketId > 0 && location.TicketId != CurrentTicket.Id)
            {
                MoveOrders(CurrentTicket.Orders.ToList(), location.TicketId);
                OpenTicket(location.TicketId);
            }

            CurrentTicket.LocationName = location.Name;
            if (_departmentService.CurrentDepartment != null) CurrentTicket.DepartmentId = _departmentService.CurrentDepartment.Id;
            location.TicketId = CurrentTicket.GetRemainingAmount() > 0 ? CurrentTicket.Id : 0;

            RuleExecutor.NotifyEvent(RuleEventNames.TicketLocationChanged, new { Ticket = CurrentTicket, OldLocation = oldLocation, NewLocation = CurrentTicket.LocationName });
        }

        public TicketCommitResult MoveOrders(IEnumerable<Order> selectedOrders, int targetTicketId)
        {
            var clonedOrders = selectedOrders.Select(ObjectCloner.Clone).ToList();
            selectedOrders.ToList().ForEach(x => CurrentTicket.Orders.Remove(x));

            if (CurrentTicket.Orders.Count == 0)
            {
                var info = targetTicketId.ToString();
                if (targetTicketId > 0)
                {
                    var tData = Dao.Single<Ticket, dynamic>(targetTicketId, x => new { x.LocationName, x.TicketNumber });
                    info = tData.LocationName + " - " + tData.TicketNumber;
                }
                if (!string.IsNullOrEmpty(CurrentTicket.Note)) CurrentTicket.Note += "\r";
                CurrentTicket.Note += CurrentTicket.LocationName + " => " + info;
            }

            CloseTicket();

            OpenTicket(targetTicketId);

            foreach (var clonedOrder in clonedOrders)
            {
                CurrentTicket.Orders.Add(clonedOrder);
            }

            CurrentTicket.LastOrderDate = DateTime.Now;
            return CloseTicket();
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(MenuItem menuItem)
        {
            return GetOrderTagGroupsForItem(_departmentService.CurrentDepartment.TicketTemplate.OrderTagGroups, menuItem);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<MenuItem> menuItems)
        {
            return menuItems.Aggregate(_departmentService.CurrentDepartment.TicketTemplate.OrderTagGroups.OrderBy(x => x.Order) as IEnumerable<OrderTagGroup>, GetOrderTagGroupsForItem);
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
            ticket.Recalculate(AppServices.SettingService.AutoRoundDiscount, AppServices.CurrentLoggedInUser.Id);
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
            Debug.Assert(ticket!= null);
            Debug.Assert(ticket.Id > 0 || ticket.Orders.Count > 0);
            if (ticket.Id == 0 && ticket.TicketNumber != null)
                _workspace.Add(ticket);
            ticket.LastUpdateTime = DateTime.Now;
            _workspace.CommitChanges();
        }
        
        public void Reset()
        {

        }
    }
}
