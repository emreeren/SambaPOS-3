using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.ViewModel;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    public class DepartmentButtonViewModel : NotificationObject
    {
        private readonly IApplicationState _applicationState;
        private readonly IWorkPeriodService _workPeriodService;
        private readonly IUserService _userService;

        public DepartmentButtonViewModel(IApplicationState applicationState,
            IWorkPeriodService workPeriodService, IUserService userService)
        {
            _applicationState = applicationState;
            _workPeriodService = workPeriodService;
            _userService = userService;
            EventServiceFactory.EventService.GetEvent<GenericEvent<IApplicationState>>().Subscribe(OnSelectedTicketChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(OnUserLoggedIn);
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

        public IEnumerable<Department> PermittedDepartments
        {
            get { return _userService.PermittedDepartments; }
        }

        public bool CanChangeDepartment
        {
            get { return _applicationState.CurrentTicket == null && _workPeriodService.IsCurrentWorkPeriodOpen; }
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
                RaisePropertyChanged(() => IsDepartmentSelectorVisible);
                RaisePropertyChanged(() => PermittedDepartments);
                RaisePropertyChanged(() => CanChangeDepartment);
            }
        }
    }
}
