using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.DashboardModule
{
    [ModuleExport(typeof(DashboardModule))]
    public class DashboardModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly DashboardView _dashboardView;

        [ImportingConstructor]
        public DashboardModule(IRegionManager regionManager, DashboardView dashboardView)
            : base(regionManager, AppScreens.Dashboard)
        {
            _regionManager = regionManager;
            _dashboardView = dashboardView;
            SetNavigationCommand(Resources.Management, Resources.Common, "Images/Tools.png", 90);
            PermissionRegistry.RegisterPermission(PermissionNames.OpenDashboard, PermissionCategories.Navigation, Resources.CanOpenDashboard);
        }

        protected override bool CanNavigate(string arg)
        {
            return AppServices.IsUserPermittedFor(PermissionNames.OpenDashboard);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            ((DashboardViewModel)_dashboardView.DataContext).Refresh();
        }
        
        public override object GetVisibleView()
        {
            return _dashboardView;
        }

        protected override void OnPreInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(DashboardView));
            _regionManager.RegisterViewWithRegion(RegionNames.UserRegion, typeof(KeyboardButtonView));
        }
    }
}
