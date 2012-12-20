using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.InventoryReports
{
    class InventoryReportViewModel : ReportViewModelBase
    {
        public InventoryReportViewModel(IUserService userService, IApplicationState applicationState, ILogService logService)
            : base(userService, applicationState, logService)
        {

        }

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");
            report.AddHeader("Samba POS");
            report.AddHeader(Resources.InventoryReport);
            report.AddHeader(string.Format(Resources.As_f, DateTime.Now));

            var lastPeriodicConsumption = ReportContext.GetCurrentPeriodicConsumption();

            var consumptionItems = lastPeriodicConsumption.PeriodicConsumptionItems;

            if (consumptionItems.Any())
            {
                report.AddColumTextAlignment("InventoryTable", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("InventoryTable", "45*", "30*", "35*");
                report.AddTable("InventoryTable", Resources.InventoryItem, Resources.Unit, Resources.Quantity);

                foreach (var costItem in consumptionItems)
                {
                    report.AddRow("InventoryTable",
                        costItem.InventoryItemName,
                        costItem.UnitName,
                        costItem.GetPhysicalInventory().ToString("#,#0.##"));
                }
            }
            else report.AddHeader(Resources.ThereAreNoCostTransactionsInThisPeriod);

            return report.Document;
        }

        protected override string GetHeader()
        {
            return Resources.InventoryReport;
        }
    }
}
