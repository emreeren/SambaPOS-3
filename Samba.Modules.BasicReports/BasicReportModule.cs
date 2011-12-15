using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.BasicReports
{
    [ModuleExport(typeof(BasicReportModule))]
    public class BasicReportModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly BasicReportView _basicReportView;

        [ImportingConstructor]
        public BasicReportModule(IRegionManager regionManager, BasicReportView basicReportView,
            IWorkPeriodService workPeriodService, IPrinterService printerService, ICashService cashService,
            IDepartmentService departmentService, IInventoryService inventoryService, IUserService userService)
            : base(regionManager, AppScreens.ReportScreen)
        {
            ReportContext.PrinterService = printerService;
            ReportContext.WorkPeriodService = workPeriodService;
            ReportContext.CashService = cashService;
            ReportContext.DepartmentService = departmentService;
            ReportContext.InventoryService = inventoryService;
            ReportContext.UserService = userService;

            _regionManager = regionManager;
            _basicReportView = basicReportView;
            SetNavigationCommand(Resources.Reports, Resources.Common, "Images/Ppt.png", 80);

            PermissionRegistry.RegisterPermission(PermissionNames.OpenReports, PermissionCategories.Navigation, Resources.CanDisplayReports);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeReportDate, PermissionCategories.Report, Resources.CanChangeReportFilter);

            RuleActionTypeRegistry.RegisterActionType("SaveReportToFile", Resources.SaveReportToFile, new { ReportName = "", FileName = "" });
            RuleActionTypeRegistry.RegisterParameterSoruce("ReportName", () => ReportContext.Reports.Select(x => x.Header));

            EventServiceFactory.EventService.GetEvent<GenericEvent<ActionData>>().Subscribe(x =>
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
                            ReportContext.CurrentWorkPeriod = ReportContext.WorkPeriodService.CurrentWorkPeriod;
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
            return (AppServices.IsUserPermittedFor(PermissionNames.OpenReports) && ReportContext.WorkPeriodService.CurrentWorkPeriod != null);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            ReportContext.ResetCache();
            ReportContext.CurrentWorkPeriod = ReportContext.WorkPeriodService.CurrentWorkPeriod;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(BasicReportView));
        }
    }
}
