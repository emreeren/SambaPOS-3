using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
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

        public void UpdateAccount(Account account)
        {
            Debug.Assert(CurrentTicket != null);
            CurrentTicket.UpdateAccount(CheckAccount(account));
        }

        public Ticket CurrentTicket { get; private set; }

        public void OpenTicket(int ticketId)
        {
            _workspace = WorkspaceFactory.Create();
            CurrentTicket = ticketId == 0
                                ? Ticket.Create(_departmentService.CurrentDepartment)
                                : _workspace.Single<Ticket>(ticket => ticket.Id == ticketId,
                                                            x => x.Orders.Select(y => y.OrderTagValues));
            if (CurrentTicket.Id == 0)
                RuleExecutor.NotifyEvent(RuleEventNames.TicketCreated, new { Ticket = CurrentTicket });
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
                var ticketId = Dao.Select<Table, int>(x => x.TicketId, x => x.Name == CurrentTicket.LocationName).FirstOrDefault();
                {
                    if (ticketId > 0)
                    {
                        result.ErrorMessage = string.Format(Resources.TableChangedRetryLastOperation_f, CurrentTicket.LocationName);
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

                UpdateTicketTable(CurrentTicket);
                if (CurrentTicket.Id > 0)  // eğer adisyonda satır yoksa ID burada 0 olmalı.
                    _workspace.CommitChanges();
                Debug.Assert(CurrentTicket.Orders.Count(x => x.OrderNumber == 0) == 0);
            }
            result.TicketId = CurrentTicket.Id;
            _workspace = null;

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

        private void UpdateTicketTable(Ticket ticket)
        {
            if (string.IsNullOrEmpty(ticket.LocationName)) return;
            var table = _workspace.Single<Table>(x => x.Name == ticket.LocationName);
            if (table != null)
            {
                if (ticket.IsPaid || ticket.Orders.Count == 0)
                {
                    if (table.TicketId == ticket.Id)
                    {
                        table.TicketId = 0;
                        table.IsTicketLocked = false;
                    }
                }
                else
                {
                    table.TicketId = ticket.Id;
                    table.IsTicketLocked = ticket.Locked;
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

            var table = _workspace.Single<Table>(x => x.Id == locationId);
            string oldLocation = "";

            if (!string.IsNullOrEmpty(CurrentTicket.LocationName))
            {
                oldLocation = CurrentTicket.LocationName;
                var oldTable = _workspace.Single<Table>(x => x.Name == CurrentTicket.LocationName);
                if (oldTable.TicketId == CurrentTicket.Id)
                {
                    oldTable.IsTicketLocked = false;
                    oldTable.TicketId = 0;
                }
            }

            if (table.TicketId > 0 && table.TicketId != CurrentTicket.Id)
            {
                MoveOrders(CurrentTicket.Orders.ToList(), table.TicketId);
                OpenTicket(table.TicketId);
            }

            CurrentTicket.LocationName = table.Name;
            if (_departmentService.CurrentDepartment != null) CurrentTicket.DepartmentId = _departmentService.CurrentDepartment.Id;
            table.TicketId = CurrentTicket.GetRemainingAmount() > 0 ? CurrentTicket.Id : 0;

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

        public void Reset()
        {

        }
    }
}
