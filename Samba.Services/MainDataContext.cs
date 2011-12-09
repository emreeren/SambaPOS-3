using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class TicketCommitResult
    {
        public int TicketId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class MainDataContext
    {
        private class TicketWorkspace
        {
            private IWorkspace _workspace;
            public Ticket Ticket { get; private set; }

            public void CreateTicket(Department department)
            {
                Debug.Assert(_workspace == null);
                Debug.Assert(Ticket == null);
                Debug.Assert(department != null);

                _workspace = WorkspaceFactory.Create();
                Ticket = Ticket.Create(department);
            }

            public void OpenTicket(int ticketId)
            {
                Debug.Assert(_workspace == null);
                Debug.Assert(Ticket == null);
                _workspace = WorkspaceFactory.Create();

                Ticket = _workspace.Single<Ticket>(ticket => ticket.Id == ticketId, x => x.Orders.Select(y => y.OrderTagValues));
            }

            public void CommitChanges()
            {
                Debug.Assert(_workspace != null);
                Debug.Assert(Ticket != null);
                Debug.Assert(Ticket.Id > 0 || Ticket.Orders.Count > 0);
                if (Ticket.Id == 0 && Ticket.TicketNumber != null)
                    _workspace.Add(Ticket);
                Ticket.LastUpdateTime = DateTime.Now;
                _workspace.CommitChanges();
            }

            public void Reset()
            {
                Debug.Assert(Ticket != null);
                Debug.Assert(_workspace != null);
                Ticket = null;
                _workspace = null;
            }

            public Table LoadTable(string locationName)
            {
                return _workspace.Single<Table>(x => x.Name == locationName);
            }

            public Account UpdateAccount(Account account)
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

            public Table GetTableWithId(int tableId)
            {
                return _workspace.Single<Table>(x => x.Id == tableId);
            }

            public Table GetTicketTable()
            {
                Debug.Assert(!string.IsNullOrEmpty(Ticket.LocationName));
                Debug.Assert(Ticket != null);
                return _workspace.Single<Table>(x => x.Name == Ticket.LocationName);
            }

            public void ResetTableData(IEntity ticket)
            {
                _workspace.All<Table>(x => x.TicketId == ticket.Id).ToList().ForEach(x => x.Reset());
            }

            public void AddItemToSelectedTicket(Order model)
            {
                _workspace.Add(model);
            }

        }

        public int AccountCount { get; set; }
        public int TableCount { get; set; }
        public string NumeratorValue { get; set; }

        public ITicketService TicketService { get; set; }
        public IDepartmentService DepartmentService { get; set; }

        private IWorkspace _tableWorkspace;
        private readonly TicketWorkspace _ticketWorkspace = new TicketWorkspace();

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = Dao.Query<AppRule>(x => x.Actions)); } }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = Dao.Query<AppAction>()); } }


        private IEnumerable<WorkPeriod> _lastTwoWorkPeriods;
        public IEnumerable<WorkPeriod> LastTwoWorkPeriods
        {
            get { return _lastTwoWorkPeriods ?? (_lastTwoWorkPeriods = GetLastTwoWorkPeriods()); }
        }

        private IEnumerable<User> _users;
        public IEnumerable<User> Users { get { return _users ?? (_users = Dao.Query<User>(x => x.UserRole)); } }

        private IEnumerable<TaxTemplate> _taxTemplates;
        public IEnumerable<TaxTemplate> TaxTemplates
        {
            get { return _taxTemplates ?? (_taxTemplates = Dao.Query<TaxTemplate>()); }
        }

        private IEnumerable<ServiceTemplate> _serviceTemplates;
        public IEnumerable<ServiceTemplate> ServiceTemplates
        {
            get { return _serviceTemplates ?? (_serviceTemplates = Dao.Query<ServiceTemplate>()); }
        }

        public WorkPeriod CurrentWorkPeriod { get { return LastTwoWorkPeriods.LastOrDefault(); } }
        public WorkPeriod PreviousWorkPeriod { get { return LastTwoWorkPeriods.Count() > 1 ? LastTwoWorkPeriods.FirstOrDefault() : null; } }

        public TableScreen SelectedTableScreen { get; set; }

        public bool IsCurrentWorkPeriodOpen
        {
            get
            {
                return CurrentWorkPeriod != null &&
                 CurrentWorkPeriod.StartDate == CurrentWorkPeriod.EndDate;
            }
        }

        public MainDataContext()
        {
            _ticketWorkspace = new TicketWorkspace();
            TicketService = ServiceLocator.Current.GetInstance(typeof(ITicketService)) as ITicketService;
            DepartmentService = ServiceLocator.Current.GetInstance(typeof(IDepartmentService)) as IDepartmentService;
        }

        private static IEnumerable<WorkPeriod> GetLastTwoWorkPeriods()
        {
            return Dao.Last<WorkPeriod>(2);
        }

        public void ResetUserData()
        {
            DepartmentService.Reset();
            ThreadPool.QueueUserWorkItem(ResetTableAndAccountCounts);
        }

        private void ResetTableAndAccountCounts(object state)
        {
            AccountCount = Dao.Count<Account>(null);
            TableCount = Dao.Count<Table>(null);
        }

        public void StartWorkPeriod(string description, decimal cashAmount, decimal creditCardAmount, decimal ticketAmount)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                _lastTwoWorkPeriods = null;

                var latestWorkPeriod = workspace.Last<WorkPeriod>();
                if (latestWorkPeriod != null && latestWorkPeriod.StartDate == latestWorkPeriod.EndDate)
                {
                    return;
                }

                var now = DateTime.Now;
                var newPeriod = new WorkPeriod
                                    {
                                        StartDate = now,
                                        EndDate = now,
                                        StartDescription = description,
                                        CashAmount = cashAmount,
                                        CreditCardAmount = creditCardAmount,
                                        TicketAmount = ticketAmount
                                    };

                workspace.Add(newPeriod);
                workspace.CommitChanges();
                _lastTwoWorkPeriods = null;
            }
        }

        public void StopWorkPeriod(string description)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var period = workspace.Last<WorkPeriod>();
                if (period.EndDate == period.StartDate)
                {
                    period.EndDate = DateTime.Now;
                    period.EndDescription = description;
                    workspace.CommitChanges();
                }
                _lastTwoWorkPeriods = null;
            }
        }

        public void UpdateTicketTable(Ticket ticket)
        {
            if (string.IsNullOrEmpty(ticket.LocationName)) return;
            var table = _ticketWorkspace.LoadTable(ticket.LocationName);
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

        public void UpdateTables(TableScreen tableScreen, int pageNo)
        {
            SelectedTableScreen = tableScreen;
            if (SelectedTableScreen != null)
            {
                IEnumerable<int> set;
                if (tableScreen.PageCount > 1)
                {
                    set = tableScreen.Tables
                        .OrderBy(x => x.Order)
                        .Skip(pageNo * tableScreen.ItemCountPerPage)
                        .Take(tableScreen.ItemCountPerPage)
                        .Select(x => x.Id);
                }
                else set = tableScreen.Tables.OrderBy(x => x.Order).Select(x => x.Id);

                var result = Dao.Select<Table, dynamic>(x => new { x.Id, Tid = x.TicketId, Locked = x.IsTicketLocked },
                                                       x => set.Contains(x.Id));

                result.ToList().ForEach(x =>
                {
                    var table = tableScreen.Tables.Single(y => y.Id == x.Id);
                    table.TicketId = x.Tid;
                    table.IsTicketLocked = x.Locked;
                });
            }
        }

        public void AssignAccountToTicket(Ticket ticket, Account account)
        {
            Debug.Assert(ticket != null);
            ticket.UpdateAccount(_ticketWorkspace.UpdateAccount(account));
        }

        public void AssignAccountToSelectedTicket(Account account)
        {
            TicketService.UpdateAccount(account);
        }

        public void AssignLocationToSelectedTicket(int locationId)
        {
            TicketService.UpdateLocation(locationId);
        }

        public void OpenTicket(int ticketId)
        {
            TicketService.OpenTicket(ticketId);
        }

        public TicketCommitResult CloseTicket()
        {
            return TicketService.CloseTicket();
        }

        public void UpdateTicketNumber(Ticket ticket)
        {
            TicketService.UpdateTicketNumber(ticket, DepartmentService.CurrentDepartment.TicketTemplate.TicketNumerator);
        }

        public void UpdateTicketNumber(Ticket ticket, Numerator numerator)
        {
            if (numerator == null) numerator = DepartmentService.CurrentDepartment.TicketTemplate.TicketNumerator;
            if (string.IsNullOrEmpty(ticket.TicketNumber))
                ticket.TicketNumber = NumberGenerator.GetNextString(numerator.Id);
        }

        public void AddPaymentToSelectedTicket(decimal tenderedAmount, DateTime date, PaymentType paymentType)
        {
            TicketService.AddPayment(tenderedAmount, date, paymentType);
        }

        public void PaySelectedTicket(PaymentType paymentType)
        {
            TicketService.PaySelectedTicket(paymentType);
        }

        public IList<Table> LoadTables(string selectedTableScreen)
        {
            if (_tableWorkspace != null)
            {
                _tableWorkspace.CommitChanges();
            }
            _tableWorkspace = WorkspaceFactory.Create();
            return _tableWorkspace.Single<TableScreen>(x => x.Name == selectedTableScreen).Tables;
        }

        public void SaveTables()
        {
            if (_tableWorkspace != null)
            {
                _tableWorkspace.CommitChanges();
                _tableWorkspace = null;
                DepartmentService.Reset();
            }
        }

        public void ResetCache()
        {
            Debug.Assert(_ticketWorkspace.Ticket == null);

            if (_tableWorkspace == null)
            {
                var selectedDepartment = DepartmentService.CurrentDepartment != null ? DepartmentService.CurrentDepartment.Id : 0;
                var selectedTableScreen = SelectedTableScreen != null ? SelectedTableScreen.Id : 0;

                SelectedTableScreen = null;
                DepartmentService.SelectDepartment(null);
                DepartmentService.Reset();
                _lastTwoWorkPeriods = null;
                _users = null;
                _rules = null;
                _actions = null;
                _taxTemplates = null;
                _serviceTemplates = null;

                DepartmentService.SelectDepartment(selectedDepartment);

                //if (selectedDepartment > 0 && Departments.Count(x => x.Id == selectedDepartment) > 0)
                //{
                //    SelectedDepartment = Departments.Single(x => x.Id == selectedDepartment);
                //    if (selectedTableScreen > 0 && SelectedDepartment.PosTableScreens.Count(x => x.Id == selectedTableScreen) > 0)
                //        SelectedTableScreen = SelectedDepartment.PosTableScreens.Single(x => x.Id == selectedTableScreen);
                //}
            }
        }

        public void OpenTicketFromTableName(string tableName)
        {
            var table = Dao.SingleWithCache<Table>(x => x.Name == tableName);
            if (table != null)
            {
                if (table.TicketId > 0)
                    OpenTicket(table.TicketId);
                AssignLocationToSelectedTicket(table.Id);
            }
        }

        public void OpenTicketFromTicketNumber(string ticketNumber)
        {
            Debug.Assert(_ticketWorkspace.Ticket == null);
            var id = Dao.Select<Ticket, int>(x => x.Id, x => x.TicketNumber == ticketNumber).FirstOrDefault();
            if (id > 0) OpenTicket(id);
        }

        public string GetUserName(int userId)
        {
            return userId > 0 ? Users.Single(x => x.Id == userId).Name : "-";
        }

        public void CreateNewTicket()
        {
            TicketService.OpenTicket(0);
        }

        public TicketCommitResult MoveOrders(IEnumerable<Order> selectedOrders, int targetTicketId)
        {
            return TicketService.MoveOrders(selectedOrders, targetTicketId);
        }

        public void ResetTableDataForSelectedTicket()
        {
            _ticketWorkspace.ResetTableData(TicketService.CurrentTicket);
            UpdateTicketTable(TicketService.CurrentTicket);
            _ticketWorkspace.CommitChanges();
        }

        public void AddItemToSelectedTicket(Order model)
        {
            _ticketWorkspace.AddItemToSelectedTicket(model);
        }

        public void Recalculate(Ticket ticket)
        {
            ticket.Recalculate(AppServices.SettingService.AutoRoundDiscount, AppServices.CurrentLoggedInUser.Id);
        }

        public TaxTemplate GetTaxTemplate(int menuItemId)
        {
            return AppServices.DataAccessService.GetMenuItem(menuItemId).TaxTemplate;
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(MenuItem menuItem)
        {
            return GetOrderTagGroupsForItem(DepartmentService.CurrentDepartment.TicketTemplate.OrderTagGroups, menuItem);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<MenuItem> menuItems)
        {
            return menuItems.Aggregate(DepartmentService.CurrentDepartment.TicketTemplate.OrderTagGroups.OrderBy(x => x.Order) as IEnumerable<OrderTagGroup>, GetOrderTagGroupsForItem);
        }

        private static IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(IEnumerable<OrderTagGroup> tagGroups, MenuItem menuItem)
        {
            var maps = tagGroups.SelectMany(x => x.OrderTagMaps);

            maps = maps
                .Where(x => x.MenuItemGroupCode == menuItem.GroupCode || x.MenuItemGroupCode == null)
                .Where(x => x.MenuItemId == menuItem.Id || x.MenuItemId == 0);

            return tagGroups.Where(x => maps.Any(y => y.OrderTagGroupId == x.Id));
        }
    }
}
