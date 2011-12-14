using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
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

            public Location LoadLocation(string locationName)
            {
                return _workspace.Single<Location>(x => x.Name == locationName);
            }

            public Location GetLocationWithId(int locationId)
            {
                return _workspace.Single<Location>(x => x.Id == locationId);
            }

            public Location GetTicketLocation()
            {
                Debug.Assert(!string.IsNullOrEmpty(Ticket.LocationName));
                Debug.Assert(Ticket != null);
                return _workspace.Single<Location>(x => x.Name == Ticket.LocationName);
            }

            public void ResetLocationData(IEntity ticket)
            {
                _workspace.All<Location>(x => x.TicketId == ticket.Id).ToList().ForEach(x => x.Reset());
            }

            public void AddItemToSelectedTicket(Order model)
            {
                _workspace.Add(model);
            }
        }

        public int AccountCount { get; set; }
        public int LocationCount { get; set; }
        public string NumeratorValue { get; set; }

        public IDepartmentService DepartmentService { get; set; }
        public IInventoryService InventoryService { get; set; }
        public IWorkPeriodService WorkPeriodService { get; set; }
        public IUserService UserService { get; set; }

        private IWorkspace _locationWorkspace;
        private readonly TicketWorkspace _ticketWorkspace = new TicketWorkspace();

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = Dao.Query<AppRule>(x => x.Actions)); } }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = Dao.Query<AppAction>()); } }

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

        public LocationScreen SelectedLocationScreen { get; set; }

        public MainDataContext()
        {
            _ticketWorkspace = new TicketWorkspace();
            DepartmentService = ServiceLocator.Current.GetInstance(typeof(IDepartmentService)) as IDepartmentService;
            InventoryService = ServiceLocator.Current.GetInstance(typeof(IInventoryService)) as IInventoryService;
            WorkPeriodService = ServiceLocator.Current.GetInstance(typeof(IWorkPeriodService)) as IWorkPeriodService;
            UserService = ServiceLocator.Current.GetInstance(typeof(IUserService)) as IUserService;
        }

        public void ResetUserData()
        {
            DepartmentService.Reset();
            ThreadPool.QueueUserWorkItem(ResetLocationAndAccountCounts);
        }

        private void ResetLocationAndAccountCounts(object state)
        {
            AccountCount = Dao.Count<Account>(null);
            LocationCount = Dao.Count<Location>(null);
        }

        public void UpdateLocations(LocationScreen locationScreen, int pageNo)
        {
            SelectedLocationScreen = locationScreen;
            if (SelectedLocationScreen != null)
            {
                IEnumerable<int> set;
                if (locationScreen.PageCount > 1)
                {
                    set = locationScreen.Locations
                        .OrderBy(x => x.Order)
                        .Skip(pageNo * locationScreen.ItemCountPerPage)
                        .Take(locationScreen.ItemCountPerPage)
                        .Select(x => x.Id);
                }
                else set = locationScreen.Locations.OrderBy(x => x.Order).Select(x => x.Id);

                var result = Dao.Select<Location, dynamic>(x => new { x.Id, Tid = x.TicketId, Locked = x.IsTicketLocked },
                                                       x => set.Contains(x.Id));

                result.ToList().ForEach(x =>
                {
                    var location = locationScreen.Locations.Single(y => y.Id == x.Id);
                    location.TicketId = x.Tid;
                    location.IsTicketLocked = x.Locked;
                });
            }
        }

        public IList<Location> LoadLocations(string selectedLocationScreen)
        {
            if (_locationWorkspace != null)
            {
                _locationWorkspace.CommitChanges();
            }
            _locationWorkspace = WorkspaceFactory.Create();
            return _locationWorkspace.Single<LocationScreen>(x => x.Name == selectedLocationScreen).Locations;
        }

        public void SaveLocations()
        {
            if (_locationWorkspace != null)
            {
                _locationWorkspace.CommitChanges();
                _locationWorkspace = null;
                DepartmentService.Reset();
            }
        }

        public void ResetCache()
        {
            Debug.Assert(_ticketWorkspace.Ticket == null);

            if (_locationWorkspace == null)
            {
                var selectedDepartment = DepartmentService.CurrentDepartment != null ? DepartmentService.CurrentDepartment.Id : 0;
                var selectedLocationScreen = SelectedLocationScreen != null ? SelectedLocationScreen.Id : 0;

                SelectedLocationScreen = null;
                DepartmentService.SelectDepartment(null);
                DepartmentService.Reset();
                InventoryService.Reset();
                WorkPeriodService.Reset();
                UserService.Reset();
                _rules = null;
                _actions = null;
                _taxTemplates = null;
                _serviceTemplates = null;

                DepartmentService.SelectDepartment(selectedDepartment);

                //if (selectedDepartment > 0 && Departments.Count(x => x.Id == selectedDepartment) > 0)
                //{
                //    SelectedDepartment = Departments.Single(x => x.Id == selectedDepartment);
                //    if (selectedLocationScreen > 0 && SelectedDepartment.PosLocationScreens.Count(x => x.Id == selectedLocationScreen) > 0)
                //        SelectedLocationScreen = SelectedDepartment.PosLocationScreens.Single(x => x.Id == selectedLocationScreen);
                //}
            }
        }

        public TaxTemplate GetTaxTemplate(int menuItemId)
        {
            return AppServices.DataAccessService.GetMenuItem(menuItemId).TaxTemplate;
        }
    }
}
