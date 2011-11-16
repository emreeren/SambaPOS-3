using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.BasicReports.Reports.CSVBuilder
{
    class CsvBuilderViewModel : ReportViewModelBase
    {
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

            var lines = ReportContext.Tickets.SelectMany(x => x.Orders, (t, ti) => new { Ticket = t, TicketItem = ti });
            var data = lines.Select(x =>
                new
                    {
                        DateTime = x.TicketItem.CreatedDateTime,
                        Date = x.TicketItem.CreatedDateTime.ToShortDateString(),
                        Time = x.TicketItem.CreatedDateTime.ToShortTimeString(),
                        x.Ticket.TicketNumber,
                        UserName = ReportContext.GetUserName(x.TicketItem.CreatingUserId),
                        Account = x.Ticket.AccountName,
                        Location = x.Ticket.LocationName,
                        x.TicketItem.OrderNumber,
                        x.TicketItem.Voided,
                        x.TicketItem.Gifted,
                        Name = x.TicketItem.MenuItemName,
                        Portion = x.TicketItem.PortionName,
                        x.TicketItem.Quantity,
                        Price = x.TicketItem.GetItemPrice(),
                        Value = x.TicketItem.GetItemValue(),
                        Discount = x.Ticket.GetPlainSum() > 0 ? x.Ticket.GetDiscountTotal() / x.Ticket.GetPlainSum() : 0,
                        Rounding = x.Ticket.GetRoundingTotal(),
                        Total = MenuGroupBuilder.CalculateTicketItemTotal(x.Ticket, x.TicketItem),
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
