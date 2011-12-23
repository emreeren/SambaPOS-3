using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Presentation.Common.Services
{
    [Export(typeof(IApplicationState))]
    [Export(typeof(IApplicationStateSetter))]
    public class ApplicationState : AbstractService, IApplicationState, IApplicationStateSetter
    {
        private readonly IDepartmentService _departmentService;
        [ImportingConstructor]
        public ApplicationState(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public AppScreens ActiveAppScreen { get; private set; }
        public Department CurrentDepartment { get; private set; }
        public Ticket CurrentTicket { get; private set; }
        public LocationScreen SelectedLocationScreen { get; private set; }

        private User _currentLoggedInUser;
        public User CurrentLoggedInUser
        {
            get { return _currentLoggedInUser ?? User.Nobody; }
            private set { _currentLoggedInUser = value; }
        }

        private IEnumerable<WorkPeriod> _lastTwoWorkPeriods;
        public IEnumerable<WorkPeriod> LastTwoWorkPeriods
        {
            get { return _lastTwoWorkPeriods ?? (_lastTwoWorkPeriods = Dao.Last<WorkPeriod>(2)); }
        }

        public WorkPeriod CurrentWorkPeriod
        {
            get { return LastTwoWorkPeriods.LastOrDefault(); }
        }

        public WorkPeriod PreviousWorkPeriod
        {
            get { return LastTwoWorkPeriods.Count() > 1 ? LastTwoWorkPeriods.FirstOrDefault() : null; }
        }

        public bool IsCurrentWorkPeriodOpen
        {
            get
            {
                return CurrentWorkPeriod != null && CurrentWorkPeriod.StartDate == CurrentWorkPeriod.EndDate;
            }
        }

        public void SetCurrentLoggedInUser(User user)
        {
            CurrentLoggedInUser = user;
        }

        public void SetCurrentTicket(Ticket ticket)
        {
            CurrentTicket = ticket;
            (this as IApplicationState)._PublishEvent(EventTopicNames.SelectedTicketChanged);
        }

        public void SetCurrentDepartment(Department department)
        {
            if (department != CurrentDepartment)
            {
                CurrentDepartment = department;
                CurrentDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
            }
        }

        public void SetCurrentDepartment(int departmentId)
        {
            SetCurrentDepartment(_departmentService.GetDepartment(departmentId));
        }

        public void SetCurrentApplicationScreen(AppScreens appScreen)
        {
            ActiveAppScreen = appScreen;
        }

        public void SetSelectedLocationScreen(LocationScreen locationScreen)
        {
            SelectedLocationScreen = locationScreen;
        }

        public void ResetWorkPeriods()
        {
            _lastTwoWorkPeriods = null;
        }

        public override void Reset()
        {
            _lastTwoWorkPeriods = null;
        }
    }
}
