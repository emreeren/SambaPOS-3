using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.DepartmentModule
{
    public class DepartmentSelectorViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly IUserService _userService;
        private readonly IApplicationStateSetter _applicationStateSetter;

        public DepartmentSelectorViewModel(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter, IUserService userService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _userService = userService;
            EventServiceFactory.EventService.GetEvent<GenericEvent<IApplicationState>>().Subscribe(OnSelectedTicketChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(OnUserLoggedIn);
        }

        public void UpdateSelectedDepartment(Department department)
        {
            _applicationStateSetter.SetCurrentDepartment(department.Id);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
            PermittedDepartments.ToList().ForEach(x => x.Refresh());
            RaisePropertyChanged(() => PermittedDepartments);
        }

        private void OnSelectedTicketChanged(EventParameters<IApplicationState> obj)
        {
            if (obj.Topic == EventTopicNames.SelectedTicketChanged)
                RaisePropertyChanged(() => CanChangeDepartment);
        }

        public bool IsDepartmentSelectorVisible
        {
            get
            {
                return PermittedDepartments.Count() > 1
                    && _userService.IsUserPermittedFor(PermissionNames.ChangeDepartment);
            }
        }

        private IList<DepartmentButtonViewModel> _permittedDepartments;
        public IEnumerable<DepartmentButtonViewModel> PermittedDepartments
        {
            get
            {
                return
                    _permittedDepartments ??
                    (_permittedDepartments = _userService.PermittedDepartments
                    .Select(x => new DepartmentButtonViewModel(this, _applicationState)
                               {
                                   Department = x,
                                   Name = x.Name
                               }).ToList());
            }
        }

        public bool CanChangeDepartment
        {
            get { return _applicationState.CurrentTicket == null && _applicationState.IsCurrentWorkPeriodOpen; }
        }

        private void OnWorkPeriodChanged(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic == EventTopicNames.WorkPeriodStatusChanged)
            {
                RaisePropertyChanged(() => CanChangeDepartment);
            }
        }

        private void OnUserLoggedIn(EventParameters<User> obj)
        {
            if (obj.Topic == EventTopicNames.UserLoggedIn)
            {
                _permittedDepartments = null;
                RaisePropertyChanged(() => IsDepartmentSelectorVisible);
                RaisePropertyChanged(() => PermittedDepartments);
                RaisePropertyChanged(() => CanChangeDepartment);
            }
        }
    }
}
