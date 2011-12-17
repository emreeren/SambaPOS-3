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
        private readonly IDepartmentService _departmentService;
        private readonly ITicketService _ticketService;
        private readonly IWorkPeriodService _workPeriodService;

        public DepartmentButtonViewModel(IDepartmentService departmentService, ITicketService ticketService, IWorkPeriodService workPeriodService)
        {
            _departmentService = departmentService;
            _ticketService = ticketService;
            _workPeriodService = workPeriodService;

            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(OnUserLoggedIn);
        }

        public bool IsDepartmentSelectorVisible
        {
            get
            {
                return PermittedDepartments.Count() > 1
                    && AppServices.IsUserPermittedFor(PermissionNames.ChangeDepartment);
            }
        }

        public IEnumerable<Department> PermittedDepartments
        {
            get { return _departmentService.GetPermittedDepartments(); }
        }

        public bool CanChangeDepartment
        {
            get { return _ticketService.CurrentTicket == null && _workPeriodService.IsCurrentWorkPeriodOpen; }
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
                _departmentService.Reset();
                RaisePropertyChanged(() => CanChangeDepartment);
                RaisePropertyChanged(() => PermittedDepartments);
                RaisePropertyChanged(() => IsDepartmentSelectorVisible);
            }
        }

    }
}
