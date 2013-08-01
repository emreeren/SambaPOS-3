using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.BasicReports.Reports.InventoryReports
{
    class PurchaseReportViewModel : ReportViewModelBase
    {
        public PurchaseReportViewModel(IUserService userService, IApplicationState applicationState, ILogService logService, ISettingService settingService)
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

            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, Resources.InventoryPurchaseReport);

            var transactionGroups = ReportContext.Transactions.SelectMany(x => x.TransactionItems)
                .GroupBy(x => new { x.InventoryItem.GroupCode })
                .Select(x => new { ItemName = x.Key.GroupCode, Total = x.Sum(y => y.Price * y.Quantity) }).ToList();

            if (transactionGroups.Any())
            {
                report.AddColumTextAlignment("GrupToplam", TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("GrupToplam", "60*", "40*");
                report.AddTable("GrupToplam", Resources.InventoryGroup, Resources.Total);

                if (transactionGroups.Count() > 1)
                {
                    foreach (var transactionItem in transactionGroups)
                    {
                        report.AddRow("GrupToplam",
                            !string.IsNullOrEmpty(transactionItem.ItemName) ? transactionItem.ItemName : Resources.UndefinedWithBrackets,
                            transactionItem.Total.ToString(ReportContext.CurrencyFormat));
                    }
                }
                report.AddRow("GrupToplam",
                    Resources.Total, transactionGroups.Sum(x => x.Total).ToString(ReportContext.CurrencyFormat));
            }

            var transactionItems = ReportContext.Transactions.SelectMany(x => x.TransactionItems)
                .GroupBy(x => new { x.InventoryItem.Name, x.Unit })
                .Select(x => new { ItemName = x.Key.Name, Quantity = x.Sum(y => y.Quantity), x.Key.Unit, Total = x.Sum(y => y.Price * y.Quantity) });

            if (transactionItems.Any())
            {
                report.AddColumTextAlignment("Alımlar", TextAlignment.Left, TextAlignment.Right, TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("Alımlar", "40*", "20*", "15*", "25*");
                report.AddTable("Alımlar", Resources.InventoryItem, Resources.Quantity, Resources.Unit, Resources.AveragePrice_ab);

                foreach (var transactionItem in transactionItems)
                {
                    report.AddRow("Alımlar",
                        transactionItem.ItemName,
                        transactionItem.Quantity.ToString("#,#0.##"),
                        transactionItem.Unit,
                        (transactionItem.Total / transactionItem.Quantity).ToString(ReportContext.CurrencyFormat));
                }
            }
            else
            {
                report.AddHeader("");
                report.AddHeader(Resources.NoPurchaseTransactionInCurrentDateRange);
            }
            return report.Document;
        }

        protected override string GetHeader()
        {
            return Resources.InventoryPurchaseReport;
        }
    }
}
