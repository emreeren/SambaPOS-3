using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.ManagementModule
{
    [ModuleExport(typeof(ManagementModule))]
    public class ManagementModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ManagementView _dashboardView;
        private readonly IUserService _userService;

        [ImportingConstructor]
        public ManagementModule(IRegionManager regionManager, ManagementView dashboardView, IUserService userService)
            : base(regionManager, AppScreens.Management)
        {
            _regionManager = regionManager;
            _dashboardView = dashboardView;
            _userService = userService;
            SetNavigationCommand(Resources.Management, Resources.Common, "Images/Tools.png", 70);
            PermissionRegistry.RegisterPermission(PermissionNames.OpenDashboard, PermissionCategories.Navigation, Resources.CanOpenDashboard);
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.OpenDashboard);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            ((ManagementViewModel)_dashboardView.DataContext).Refresh();
        }

        public override object GetVisibleView()
        {
            return _dashboardView;
        }

        protected override void OnPreInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(ManagementView));
            _regionManager.RegisterViewWithRegion(RegionNames.UserRegion, typeof(KeyboardButtonView));
        }
    }
}
