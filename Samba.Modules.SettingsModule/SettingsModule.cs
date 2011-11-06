using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Localization.Properties;
using Samba.Modules.SettingsModule.WorkPeriods;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    [ModuleExport(typeof(SettingsModule))]
    public class SettingsModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly WorkPeriodsView _workPeriodsView;

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(WorkPeriodsView));
        }

        [ImportingConstructor]
        public SettingsModule(IRegionManager regionManager, WorkPeriodsView workPeriodsView)
            : base(regionManager, AppScreens.WorkPeriods)
        {
            _regionManager = regionManager;
            _workPeriodsView = workPeriodsView;

            SetNavigationCommand(Resources.DayOperations, Resources.Common, "Images/Run.png");

            AddDashboardCommand<SettingsViewModel>(Resources.LocalSettings, Resources.Settings);
            AddDashboardCommand<TerminalListViewModel>(Resources.Terminals, Resources.Settings);
            AddDashboardCommand<PrinterListViewModel>(Resources.Printers, Resources.Settings);
            AddDashboardCommand<PrintJobListViewModel>(Resources.PrintJobs, Resources.Settings);
            AddDashboardCommand<PrinterTemplateCollectionViewModel>(Resources.PrinterTemplates, Resources.Settings);
            AddDashboardCommand<NumeratorListViewModel>(Resources.Numerators, Resources.Settings);
            AddDashboardCommand<VoidReasonListViewModel>(Resources.VoidReasons, Resources.Products);
            AddDashboardCommand<GiftReasonListViewModel>(Resources.GiftReasons, Resources.Products);
            AddDashboardCommand<ProgramSettingsViewModel>(Resources.ProgramSettings, Resources.Settings, 10);
            AddDashboardCommand<RuleActionListViewModel>(Resources.RuleActions, Resources.Settings);
            AddDashboardCommand<RuleListViewModel>(Resources.Rules, Resources.Settings);
            AddDashboardCommand<TriggerListViewModel>(Resources.Triggers, Resources.Settings);
            AddDashboardCommand<BrowserViewModel>(Resources.SambaPosWebsite, Resources.SambaNetwork, 99);

            PermissionRegistry.RegisterPermission(PermissionNames.OpenWorkPeriods, PermissionCategories.Navigation,
                                                  Resources.CanStartEndOfDay);
        }

        public override object GetVisibleView()
        {
            return _workPeriodsView;
        }

        protected override bool CanNavigate(string arg)
        {
            return AppServices.IsUserPermittedFor(PermissionNames.OpenWorkPeriods);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            ((WorkPeriodsViewModel)_workPeriodsView.DataContext).Refresh();
        }

        //private void OnShowBrowser(string obj)
        //{
        //    if (_browserViewModel == null)
        //        _browserViewModel = new BrowserViewModel();
        //    CommonEventPublisher.PublishViewAddedEvent(_browserViewModel);
        //    new Uri("http://network.sambapos.com").PublishEvent(EventTopicNames.BrowseUrl);
        //}
    }
}
