using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.Services
{
    [Export(typeof(IApplicationState))]
    [Export(typeof(IApplicationStateSetter))]
    public class ApplicationState : AbstractService, IApplicationState, IApplicationStateSetter
    {
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public ApplicationState(IDepartmentService departmentService, ISettingService settingService)
        {
            _departmentService = departmentService;
            _settingService = settingService;
        }

        public AppScreens ActiveAppScreen { get; private set; }
        public CurrentDepartmentData CurrentDepartment { get; private set; }
        public ResourceScreen SelectedResourceScreen { get; private set; }

        private Terminal _terminal;

        private bool _isLocked;
        public bool IsLocked
        {
            get { return _isLocked; }
        }

        public Terminal CurrentTerminal { get { return _terminal ?? (_terminal = GetCurrentTerminal()); } set { _terminal = value; } }

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

        public void SetCurrentDepartment(Department department)
        {
            if (CurrentDepartment == null || department != CurrentDepartment.Model)
            {
                CurrentDepartment = new CurrentDepartmentData { Model = department };
                CurrentDepartment.Model.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
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

        public void SetSelectedResourceScreen(ResourceScreen resourceScreen)
        {
            SelectedResourceScreen = resourceScreen;
        }

        public void SetApplicationLocked(bool isLocked)
        {
            _isLocked = isLocked;
            (this as IApplicationState).PublishEvent(EventTopicNames.ApplicationLockStateChanged);
        }

        private Terminal GetCurrentTerminal()
        {
            if (!string.IsNullOrEmpty(LocalSettings.TerminalName))
            {
                var terminal = _settingService.GetTerminalByName(LocalSettings.TerminalName);
                if (terminal != null) return terminal;
            }
            var dterminal = _settingService.GetDefaultTerminal();
            return dterminal ?? Terminal.DefaultTerminal;
        }

        public void ResetWorkPeriods()
        {
            _lastTwoWorkPeriods = null;
        }

        public override void Reset()
        {
            _lastTwoWorkPeriods = null;
            _terminal = null;
        }

        public void ResetState()
        {
            _departmentService.Reset();

            var did = CurrentDepartment.Id;
            CurrentDepartment = null;
            SetCurrentDepartment(did);

            var rid = SelectedResourceScreen.Id;
            if (CurrentDepartment != null)
                SelectedResourceScreen = CurrentDepartment.ResourceScreens.Single(x => x.Id == rid);
        }
    }
}
