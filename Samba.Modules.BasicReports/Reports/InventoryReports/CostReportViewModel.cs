using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.BasicReports.Reports.InventoryReports
{
    class CostReportViewModel : ReportViewModelBase
    {
        public CostReportViewModel(IUserService userService, IApplicationState applicationState, ILogService logService, ISettingService settingService)
            : base(userService, applicationState, logService, settingService)
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
            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, Resources.CostReport);

            var costItems = ReportContext.PeriodicConsumptions.SelectMany(x => x.WarehouseConsumptions).SelectMany(x => x.CostItems)
                .GroupBy(x => new { ItemName = x.Name, x.PortionName })
                .Select(x => new { x.Key.ItemName, x.Key.PortionName, TotalQuantity = x.Sum(y => y.Quantity), TotalCost = x.Sum(y => y.Cost * y.Quantity) }).ToList();

            if (costItems.Any())
            {
                report.AddColumTextAlignment("Maliyet", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("Maliyet", "38*", "20*", "17*", "25*");
                report.AddTable("Maliyet", Resources.MenuItem, Resources.Portion, Resources.Quantity, Resources.AverageCost);

                foreach (var costItem in costItems)
                {
                    report.AddRow("Maliyet",
                        costItem.ItemName,
                        costItem.PortionName,
                        costItem.TotalQuantity.ToString("#,#0.##"),
                        (costItem.TotalCost / costItem.TotalQuantity).ToString(ReportContext.CurrencyFormat));
                }

                report.AddRow("Maliyet", Resources.Total, "", "", costItems.Sum(x => x.TotalCost).ToString(ReportContext.CurrencyFormat));
            }
            else report.AddHeader(Resources.ThereAreNoCostTransactionsInThisPeriod);

            return report.Document;
        }

        protected override string GetHeader()
        {
            return Resources.CostReport;
        }
    }
}
