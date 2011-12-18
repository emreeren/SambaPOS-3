using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Presentation.Common.Services
{
    [Export(typeof(IApplicationState))]
    [Export(typeof(IApplicationStateSetter))]
    public class ApplicationState : AbstractService, IApplicationState, IApplicationStateSetter
    {
        public Ticket CurrentTicket { get; private set; }

        public void SetCurrentTicket(Ticket ticket)
        {
            CurrentTicket = ticket;
            (this as IApplicationState)._PublishEvent(EventTopicNames.SelectedTicketChanged);
        }

        private User _currentLoggedInUser;
        public User CurrentLoggedInUser
        {
            get { return _currentLoggedInUser ?? User.Nobody; }
            private set { _currentLoggedInUser = value; }
        }

        public void SetCurrentLoggedInUser(User user)
        {
            CurrentLoggedInUser = user;
        }

        private IWorkspace _workspace;
        public IWorkspace Workspace
        {
            get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); }
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = Workspace.All<Department>()); }
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
            SetCurrentDepartment(Departments.First(x => x.Id == departmentId));
        }

        public void SetCurrentApplicationScreen(AppScreens appScreen)
        {
            ActiveAppScreen = appScreen;
        }

        public AppScreens ActiveAppScreen { get; private set; }

        public Department CurrentDepartment { get; private set; }

        public override void Reset()
        {
            _departments = null;
            _workspace = null;
        }
    }
}
