using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    public class DepartmentSelectorViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly IUserService _userService;
        private readonly IApplicationStateSetter _applicationStateSetter;

        public DepartmentSelectorViewModel(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IUserService userService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _userService = userService;
            EventServiceFactory.EventService.GetEvent<GenericEvent<IApplicationState>>().Subscribe(OnSelectedTicketChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(OnUserLoggedIn);
        }

        public void UpdateSelectedDepartment(int departmentId)
        {
            _applicationStateSetter.SetSelectedEntityScreen(null);
            _applicationStateSetter.SetCurrentDepartment(departmentId);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
            PermittedDepartments.ToList().ForEach(x => x.Refresh());
            RaisePropertyChanged(() => PermittedDepartments);
        }

        private void OnSelectedTicketChanged(EventParameters<IApplicationState> obj)
        {
            if (obj.Topic == EventTopicNames.ApplicationLockStateChanged)
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
                                   DepartmentId = x.Id,
                                   Name = x.Name
                               }).ToList());
            }
        }

        public bool CanChangeDepartment
        {
            get { return !_applicationState.IsLocked && _applicationState.IsCurrentWorkPeriodOpen; }
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
                if (obj.Value.UserRole.DepartmentId > 0)
                {
                    _applicationStateSetter.SetSelectedEntityScreen(null);
                    _applicationStateSetter.SetCurrentDepartment(obj.Value.UserRole.DepartmentId);
                    //_applicationStateSetter.SetCurrentTicketType(_cacheService.GetTicketTypeById(_applicationState.CurrentDepartment.TicketTypeId));
                }
            }
        }
    }
}
