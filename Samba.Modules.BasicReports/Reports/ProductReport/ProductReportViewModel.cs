using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.ProductReport
{
    public class ProductReportViewModel : ReportViewModelBase
    {
        public ProductReportViewModel(IUserService userService, IApplicationState applicationState)
            : base(userService, applicationState)
        {
        }

        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");

            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, Resources.ItemSalesReport);

            var menuGroups = MenuGroupBuilder.CalculateMenuGroups(ReportContext.Tickets, ReportContext.MenuItems).ToList();

            report.AddColumTextAlignment("ÜrünGrubu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("ÜrünGrubu", "40*", "Auto", "35*");
            report.AddTable("ÜrünGrubu", Resources.SalesByItemGroup, "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("ÜrünGrubu", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.Rate),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("ÜrünGrubu", Resources.Total, "", menuGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));


            //----------------------

            report.AddColumTextAlignment("ÜrünGrubuMiktar", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("ÜrünGrubuMiktar", "40*", "Auto", "35*");
            report.AddTable("ÜrünGrubuMiktar", Resources.QuantitiesByItemGroup, "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("ÜrünGrubuMiktar", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.QuantityRate),
                    menuItemInfo.Quantity.ToString("#"));
            }

            report.AddRow("ÜrünGrubuMiktar", Resources.Total, "", menuGroups.Sum(x => x.Quantity).ToString("#"));


            //----------------------

            var menuItems = MenuGroupBuilder.CalculateMenuItems(ReportContext.Tickets, ReportContext.MenuItems)
                .OrderByDescending(x => x.Quantity);

            report.AddColumTextAlignment("ÜrünTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("ÜrünTablosu", "50*", "Auto", "25*");
            report.AddTable("ÜrünTablosu", Resources.MenuItem, Resources.Quantity, Resources.Amount);

            foreach (var menuItemInfo in menuItems)
            {
                report.AddRow("ÜrünTablosu",
                    menuItemInfo.Name,
                    string.Format("{0:0.##}", menuItemInfo.Quantity),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("ÜrünTablosu", Resources.Total, "", menuItems.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));


            //----------------------

            var returnedMenuItems = MenuGroupBuilder.CalculateReturnedItems(ReportContext.Tickets, ReportContext.MenuItems)
                .OrderByDescending(x => x.Quantity);
            if (returnedMenuItems.Any())
            {
                report.AddColumTextAlignment("IadeTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("IadeTablosu", "50*", "Auto", "25*");
                report.AddTable("IadeTablosu", Resources.MenuItem, Resources.Quantity, Resources.Amount);

                foreach (var menuItemInfo in returnedMenuItems)
                {
                    report.AddRow("IadeTablosu",
                                  menuItemInfo.Name,
                                  string.Format("{0:0.##}", menuItemInfo.Quantity),
                                  menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
                }

                report.AddRow("IadeTablosu", Resources.Total, "",
                              returnedMenuItems.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            }

            //----------------------


            //PrepareModificationTable(report, x => x.Voided, Resources.Voids);
            //PrepareModificationTable(report, x => x.Gifted, Resources.Gifts);

            //var discounts = ReportContext.Tickets
            //    .SelectMany(x => x.Discounts.Select(y => new { x.TicketNumber, y.UserId, Amount = y.DiscountAmount }))
            //    .GroupBy(x => new { x.TicketNumber, x.UserId }).Select(x => new { x.Key.TicketNumber, x.Key.UserId, Amount = x.Sum(y => y.Amount) });

            //if (discounts.Count() > 0)
            //{
            //    report.AddColumTextAlignment("İskontolarTablosu", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
            //    report.AddColumnLength("İskontolarTablosu", "20*", "Auto", "35*");
            //    report.AddTable("İskontolarTablosu", Resources.Discounts, "", "");

            //    foreach (var discount in discounts.OrderByDescending(x => x.Amount))
            //    {
            //        report.AddRow("İskontolarTablosu", discount.TicketNumber, ReportContext.GetUserName(discount.UserId), discount.Amount.ToString(ReportContext.CurrencyFormat));
            //    }

            //    if (discounts.Count() > 1)
            //        report.AddRow("İskontolarTablosu", Resources.Total, "", discounts.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            //}

            //----------------------

            //var ticketGroups = ReportContext.Tickets
            //    .GroupBy(x => new { x.DepartmentId })
            //    .Select(x => new { x.Key.DepartmentId, TicketCount = x.Count(), Amount = x.Sum(y => y.GetSumWithoutTax()) });

            //if (ticketGroups.Count() > 0)
            //{

            //    report.AddColumTextAlignment("AdisyonlarTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            //    report.AddColumnLength("AdisyonlarTablosu", "40*", "20*", "40*");
            //    report.AddTable("AdisyonlarTablosu", Resources.Tickets, "", "");

            //    foreach (var ticketGroup in ticketGroups)
            //    {
            //        report.AddRow("AdisyonlarTablosu", ReportContext.GetDepartmentName(ticketGroup.DepartmentId), ticketGroup.TicketCount.ToString("#.##"), ticketGroup.Amount.ToString(ReportContext.CurrencyFormat));
            //    }

            //    if (ticketGroups.Count() > 1)
            //        report.AddRow("AdisyonlarTablosu", Resources.Total, ticketGroups.Sum(x => x.TicketCount).ToString("#.##"), ticketGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            //}

            //----------------------

            var properties = ReportContext.Tickets
                .SelectMany(x => x.Orders.Where(y => y.OrderTagValues.Count > 0))
                .SelectMany(x => x.OrderTagValues.Where(y => y.MenuItemId == 0).Select(y => new { Name = y.TagValue, x.Quantity }))
                .GroupBy(x => new { x.Name })
                .Select(x => new { x.Key.Name, Quantity = x.Sum(y => y.Quantity) }).ToList();

            if (properties.Any())
            {

                report.AddColumTextAlignment("ÖzelliklerTablosu", TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("ÖzelliklerTablosu", "60*", "40*");
                report.AddTable("ÖzelliklerTablosu", Resources.Properties, "");

                foreach (var property in properties.OrderByDescending(x => x.Quantity))
                {
                    report.AddRow("ÖzelliklerTablosu", property.Name, property.Quantity.ToString("#.##"));
                }
            }
            return report.Document;
        }

        //private static void PrepareModificationTable(SimpleReport report, Func<Order, bool> predicate, string title)
        //{
        //    var modifiedItems = ReportContext.Tickets
        //        .SelectMany(x => x.Orders.Where(predicate).Select(y => new { Ticket = x, UserId = y.ModifiedUserId, MenuItem = y.MenuItemName, y.Quantity, y.ModifiedDateTime, Amount = y.GetItemValue() }));

        //    if (modifiedItems.Count() == 0) return;

        //    report.AddColumTextAlignment(title, TextAlignment.Left, TextAlignment.Left, TextAlignment.Left, TextAlignment.Left);
        //    report.AddColumnLength(title, "14*", "45*", "28*", "13*");
        //    report.AddTable(title, title, "", "", "");

        //    foreach (var voidItem in modifiedItems)
        //    {
        //        report.AddRow(title, voidItem.Ticket.TicketNumber, voidItem.Quantity.ToString("#.##") + " " + voidItem.MenuItem, ReportContext.GetUserName(voidItem.UserId), voidItem.ModifiedDateTime.ToShortTimeString());
        //    }

        //    var voidGroups =
        //        from c in modifiedItems
        //        group c by c.UserId into grp
        //        select new { UserId = grp.Key, Amount = grp.Sum(x => x.Amount) };

        //    report.AddColumTextAlignment("Personel" + title, TextAlignment.Left, TextAlignment.Right);
        //    report.AddColumnLength("Personel" + title, "60*", "40*");
        //    report.AddTable("Personel" + title, string.Format(Resources.ByPersonnel_f, title), "");

        //    foreach (var voidItem in voidGroups.OrderByDescending(x => x.Amount))
        //    {
        //        report.AddRow("Personel" + title, ReportContext.GetUserName(voidItem.UserId), voidItem.Amount.ToString(ReportContext.CurrencyFormat));
        //    }

        //    if (voidGroups.Count() > 1)
        //        report.AddRow("Personel" + title, Resources.Total, voidGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
        //}

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override string GetHeader()
        {
            return Resources.ItemSalesReport;
        }
    }
}
