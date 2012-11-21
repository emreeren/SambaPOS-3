using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.CSVBuilder
{
    class CsvBuilderViewModel : ReportViewModelBase
    {
        public CsvBuilderViewModel(IUserService userService, IApplicationState applicationState, ILogService logService)
            : base(userService, applicationState,logService)
        {
        }

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override FlowDocument GetReport()
        {
            var currentPeriod = ReportContext.CurrentWorkPeriod;
            var report = new SimpleReport("8cm");
            AddDefaultReportHeader(report, currentPeriod, Resources.CsvBuilder);
            report.Header.TextAlignment = TextAlignment.Left;
            report.AddHeader("");
            report.AddHeader(Resources.ClickLinksToExportData);
            report.AddHeader("");
            report.AddLink(Resources.ExportSalesData);
            HandleLink(Resources.ExportSalesData);

            return report.Document;
        }

        protected override void HandleClick(string text)
        {
            if (text == Resources.ExportSalesData)
            {
                ExportSalesData();
            }
        }

        private void ExportSalesData()
        {
            var fileName = AskFileName(
                    Resources.ExportSalesData + "_" + DateTime.Now.ToString().Replace(":", "").Replace(" ", "_"), ".csv");

            if (string.IsNullOrEmpty(fileName)) return;

            var lines = ReportContext.Tickets.SelectMany(x => x.Orders, (t, ti) => new { Ticket = t, Order = ti });
            var data = lines.Select(x =>
                new
                    {
                        DateTime = x.Order.CreatedDateTime,
                        Date = x.Order.CreatedDateTime.ToShortDateString(),
                        Time = x.Order.CreatedDateTime.ToShortTimeString(),
                        x.Ticket.TicketNumber,
                        UserName = x.Order.CreatingUserName,
                        Account = x.Ticket.AccountName,
                        //TargetAccount = x.Ticket.TargetAccountName,
                        x.Order.OrderNumber,
                        x.Order.CalculatePrice,
                        DecreaseFromInventory = x.Order.DecreaseInventory,
                        Name = x.Order.MenuItemName,
                        Portion = x.Order.PortionName,
                        x.Order.Quantity,
                        Price = x.Order.GetItemPrice(),
                        Value = x.Order.GetItemValue(),
                        //Discount = x.Ticket.GetPlainSum() > 0 ? x.Ticket.GetDiscountTotal() / x.Ticket.GetPlainSum() : 0,
                        //Rounding = x.Ticket.GetRoundingTotal(),
                        Total = MenuGroupBuilder.CalculateOrderTotal(x.Ticket, x.Order),
                    }
                );
            var csv = data.AsCsv();
            File.WriteAllText(fileName, csv);
        }

        protected override string GetHeader()
        {
            return Resources.CsvBuilder;
        }
    }
}
