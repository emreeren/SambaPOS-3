using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.WorkperiodModule
{
    [ModuleExport(typeof(WorkPeriodsModule))]
    public class WorkPeriodsModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly WorkPeriodsView _workPeriodsView;
        private readonly IUserService _userService;

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(WorkPeriodsView));
        }

        [ImportingConstructor]
        public WorkPeriodsModule(IRegionManager regionManager, WorkPeriodsView workPeriodsView,IUserService userService)
            : base(regionManager, AppScreens.WorkPeriods)
        {
            _regionManager = regionManager;
            _workPeriodsView = workPeriodsView;
            _userService = userService;

            SetNavigationCommand(Resources.DayOperations, Resources.Common, "Images/Run.png");
            PermissionRegistry.RegisterPermission(PermissionNames.OpenWorkPeriods, PermissionCategories.Navigation,
                                                  Resources.CanStartEndOfDay);
        }

        public override object GetVisibleView()
        {
            return _workPeriodsView;
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.OpenWorkPeriods);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            ((WorkPeriodsViewModel)_workPeriodsView.DataContext).Refresh();
        }
    }
}
