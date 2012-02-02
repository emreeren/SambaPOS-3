using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.BasicReports
{
    [ModuleExport(typeof(BasicReportModule))]
    public class BasicReportModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly BasicReportView _basicReportView;
        private readonly IUserService _userService;

        [ImportingConstructor]
        public BasicReportModule(IRegionManager regionManager, BasicReportView basicReportView,
            IWorkPeriodService workPeriodService, IPrinterService printerService, 
            IDepartmentService departmentService, IInventoryService inventoryService, IUserService userService,
            IApplicationState applicationState,IAutomationService automationService)
            : base(regionManager, AppScreens.ReportScreen)
        {
            ReportContext.PrinterService = printerService;
            ReportContext.WorkPeriodService = workPeriodService;
            ReportContext.DepartmentService = departmentService;
            ReportContext.InventoryService = inventoryService;
            ReportContext.UserService = userService;
            ReportContext.ApplicationState = applicationState;
            
            _userService = userService;

            _regionManager = regionManager;
            _basicReportView = basicReportView;
            SetNavigationCommand(Resources.Reports, Resources.Common, "Images/Ppt.png", 80);

            PermissionRegistry.RegisterPermission(PermissionNames.OpenReports, PermissionCategories.Navigation, Resources.CanDisplayReports);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeReportDate, PermissionCategories.Report, Resources.CanChangeReportFilter);

            automationService.RegisterActionType("SaveReportToFile", Resources.SaveReportToFile, new { ReportName = "", FileName = "" });
            automationService.RegisterParameterSoruce("ReportName", () => ReportContext.Reports.Select(x => x.Header));

            EventServiceFactory.EventService.GetEvent<GenericEvent<IActionData>>().Subscribe(x =>
            {
                if (x.Value.Action.ActionType == "SaveReportToFile")
                {
                    var reportName = x.Value.GetAsString("ReportName");
                    var fileName = x.Value.GetAsString("FileName");
                    if (!string.IsNullOrEmpty(reportName))
                    {
                        var report = ReportContext.Reports.Where(y => y.Header == reportName).FirstOrDefault();
                        if (report != null)
                        {
                            ReportContext.CurrentWorkPeriod = ReportContext.ApplicationState.CurrentWorkPeriod;
                            var document = report.GetReportDocument();
                            ReportViewModelBase.SaveAsXps(fileName, document);
                        }
                    }
                }
            });
        }

        public override object GetVisibleView()
        {
            return _basicReportView;
        }

        protected override bool CanNavigate(string arg)
        {
            return (_userService.IsUserPermittedFor(PermissionNames.OpenReports) 
                && ReportContext.ApplicationState.CurrentWorkPeriod != null);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            ReportContext.ResetCache();
            ReportContext.CurrentWorkPeriod = ReportContext.ApplicationState.CurrentWorkPeriod;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(BasicReportView));
        }
    }
}
