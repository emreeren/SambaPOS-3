using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Modules.BasicReports.Reports.EndOfDayReport
{
    public class EndDayReportViewModel : ReportViewModelBase
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
            AddDefaultReportHeader(report, currentPeriod, Resources.WorkPeriodReport);

            //---------------

            report.AddColumTextAlignment("Departman", TextAlignment.Left, TextAlignment.Right);
            report.AddTable("Departman", Resources.Sales, "");

            var ticketGropus = ReportContext.Tickets
                .GroupBy(x => new { x.DepartmentId })
                .Select(x => new DepartmentInfo
                {
                    DepartmentId = x.Key.DepartmentId,
                    TicketCount = x.Count(),
                    Amount = x.Sum(y => y.GetSumWithoutTax()),
                    Tax = x.Sum(y => y.CalculateTax()),
                    Services = x.Sum(y => y.GetServicesTotal())
                });

            if (ticketGropus.Count() > 1)
            {
                foreach (var departmentInfo in ticketGropus)
                {
                    report.AddRow("Departman", departmentInfo.DepartmentName, departmentInfo.Amount.ToString(ReportContext.CurrencyFormat));
                }
            }

            report.AddRow("Departman", Resources.TotalSales.ToUpper(), ticketGropus.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));

            var vatSum = ticketGropus.Sum(x => x.Tax);
            var serviceSum = ticketGropus.Sum(x => x.Services);
            if (vatSum > 0 || serviceSum > 0)
            {
                if (vatSum > 0)
                    report.AddRow("Departman", Resources.TaxTotal.ToUpper(), vatSum.ToString(ReportContext.CurrencyFormat));

                if (serviceSum > 0)
                {
                    ReportContext.Tickets.SelectMany(x => x.Services).GroupBy(x => x.ServiceId).ToList().ForEach(
                        x =>
                        {
                            var template = ReportContext.ServiceTemplates.SingleOrDefault(y => y.Id == x.Key);
                            var title = template != null ? template.Name : Resources.UndefinedWithBrackets;
                            report.AddRow("Departman", title, x.Sum(y => y.CalculationAmount).ToString(ReportContext.CurrencyFormat));
                        });
                }

                report.AddRow("Departman", Resources.GrandTotal.ToUpper(), ticketGropus.Sum(x => x.Amount + x.Tax + x.Services).ToString(ReportContext.CurrencyFormat));
            }

            //---------------

            var ac = ReportContext.GetOperationalAmountCalculator();

            report.AddColumnLength("GelirlerTablosu", "45*", "Auto", "35*");
            report.AddColumTextAlignment("GelirlerTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddTable("GelirlerTablosu", Resources.Incomes, "", "");
            report.AddRow("GelirlerTablosu", Resources.Cash, ac.CashPercent, ac.CashTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.CreditCard, ac.CreditCardPercent, ac.CreditCardTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.Voucher, ac.TicketPercent, ac.TicketTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.AccountBalance, ac.AccountPercent, ac.AccountTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.TotalIncome.ToUpper(), "", ac.TotalAmount.ToString(ReportContext.CurrencyFormat));

            //---------------

            var propertySum = ReportContext.Tickets
                .SelectMany(x => x.Orders)
                .Sum(x => x.GetOrderTagPrice() * x.Quantity);

            var discounts = ReportContext.Tickets
                .SelectMany(x => x.Discounts)
                .Sum(x => x.DiscountAmount);

            report.AddColumTextAlignment("Bilgi", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Bilgi", "65*", "35*");
            report.AddTable("Bilgi", Resources.GeneralInformation, "");
            report.AddRow("Bilgi", Resources.ItemProperties, propertySum.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", Resources.DiscountsTotal, discounts.ToString(ReportContext.CurrencyFormat));

            if (ticketGropus.Count() > 1)
                foreach (var departmentInfo in ticketGropus)
                {
                    report.AddRow("Bilgi", departmentInfo.DepartmentName, departmentInfo.TicketCount);
                }

            var ticketCount = ticketGropus.Sum(x => x.TicketCount);

            report.AddRow("Bilgi", Resources.TicketCount, ticketCount);

            report.AddRow("Bilgi", Resources.SalesDivTicket, ticketCount > 0
                ? (ticketGropus.Sum(x => x.Amount) / ticketGropus.Sum(x => x.TicketCount)).ToString(ReportContext.CurrencyFormat)
                : "0");

            if (ticketGropus.Count() > 1)
            {
                foreach (var departmentInfo in ticketGropus)
                {
                    var dinfo = departmentInfo;

                    var dPayments = ReportContext.Tickets
                        .Where(x => x.DepartmentId == dinfo.DepartmentId)
                        .SelectMany(x => x.Payments)
                        .GroupBy(x => new { x.PaymentType })
                        .Select(x => new TenderedAmount { PaymentType = x.Key.PaymentType, Amount = x.Sum(y => y.Amount) });

                    report.AddColumnLength(departmentInfo.DepartmentName + Resources.Incomes, "40*", "Auto", "35*");
                    report.AddColumTextAlignment(departmentInfo.DepartmentName + Resources.Incomes, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                    report.AddTable(departmentInfo.DepartmentName + Resources.Incomes, string.Format(Resources.Incomes_f, departmentInfo.DepartmentName), "", "");
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.Cash, GetPercent(0, dPayments), GetAmount(0, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.CreditCard, GetPercent(1, dPayments), GetAmount(1, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.Voucher, GetPercent(2, dPayments), GetAmount(2, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.TotalIncome, "", departmentInfo.Amount.ToString(ReportContext.CurrencyFormat));

                    var ddiscounts = ReportContext.Tickets
                        .Where(x => x.DepartmentId == dinfo.DepartmentId)
                        .SelectMany(x => x.Discounts)
                        .Sum(x => x.DiscountAmount);

                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.DiscountsTotal, "", ddiscounts.ToString(ReportContext.CurrencyFormat));
                }
            }

            //--

            if (ReportContext.Tickets.Select(x => x.GetTagData()).Where(x => !string.IsNullOrEmpty(x)).Distinct().Count() > 0)
            {
                var dict = new Dictionary<string, List<Ticket>>();

                foreach (var ticket in ReportContext.Tickets.Where(x => x.IsTagged))
                {
                    foreach (var tag in ticket.Tags.Select(x => x.TagName + ":" + x.TagValue))
                    {
                        if (!dict.ContainsKey(tag))
                            dict.Add(tag, new List<Ticket>());
                        dict[tag].Add(ticket);
                    }
                }

                var tagGroups = dict.Select(x => new TicketTagInfo { Amount = x.Value.Sum(y => y.GetSumWithoutTax()), TicketCount = x.Value.Count, TagName = x.Key }).OrderBy(x => x.TagName);

                var tagGrp = tagGroups.GroupBy(x => x.TagName.Split(':')[0]);

                report.AddColumTextAlignment("Etiket", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("Etiket", "45*", "Auto", "35*");
                report.AddTable("Etiket", Resources.TicketTags, "", "");

                foreach (var grp in tagGrp)
                {
                    var grouping = grp;
                    var tag = ReportContext.TicketTagGroups.SingleOrDefault(x => x.Name == grouping.Key);
                    if (tag == null) continue;

                    report.AddBoldRow("Etiket", grp.Key, "", "");

                    if (tag.IsDecimal)
                    {
                        var tCount = grp.Sum(x => x.TicketCount);
                        var tSum = grp.Sum(x => Convert.ToDecimal(x.TagName.Split(':')[1]) * x.TicketCount);
                        var amnt = grp.Sum(x => x.Amount);
                        var rate = tSum / amnt;
                        report.AddRow("Etiket", string.Format(Resources.TotalAmount_f, tag.Name), "", tSum.ToString(ReportContext.CurrencyFormat));
                        report.AddRow("Etiket", Resources.TicketCount, "", tCount);
                        report.AddRow("Etiket", Resources.TicketTotal, "", amnt.ToString(ReportContext.CurrencyFormat));
                        report.AddRow("Etiket", Resources.Rate, "", rate.ToString("%#0.##"));
                        continue;
                    }

                    foreach (var ticketTagInfo in grp)
                    {
                        report.AddRow("Etiket",
                            ticketTagInfo.TagName.Split(':')[1],
                            ticketTagInfo.TicketCount,
                            ticketTagInfo.Amount.ToString(ReportContext.CurrencyFormat));
                    }


                    var totalAmount = grp.Sum(x => x.Amount);
                    report.AddRow("Etiket", string.Format(Resources.TotalAmount_f, tag.Name), "", totalAmount.ToString(ReportContext.CurrencyFormat));

                    var sum = 0m;

                    if (tag.IsInteger)
                    {
                        try
                        {
                            sum = grp.Sum(x => Convert.ToDecimal(x.TagName.Split(':')[1]) * x.TicketCount);
                            report.AddRow("Etiket", string.Format(Resources.TicketTotal_f, tag.Name), "", sum.ToString("#,##.##"));
                        }
                        catch (FormatException)
                        {
                            report.AddRow("Etiket", string.Format(Resources.TicketTotal_f, tag.Name), "", "#Hata!");
                        }
                    }
                    else
                    {
                        sum = grp.Sum(x => x.TicketCount);
                    }
                    if (sum > 0)
                    {
                        var average = totalAmount / sum;
                        report.AddRow("Etiket", string.Format(Resources.TotalAmountDivTag_f, tag.Name), "", average.ToString(ReportContext.CurrencyFormat));
                    }
                }
            }

            //----

            var owners = ReportContext.Tickets.SelectMany(ticket => ticket.Orders.Select(order => new { Ticket = ticket, Order = order }))
                .GroupBy(x => new { x.Order.CreatingUserId })
                .Select(x => new UserInfo { UserId = x.Key.CreatingUserId, Amount = x.Sum(y => MenuGroupBuilder.CalculateOrderTotal(y.Ticket, y.Order)) });

            report.AddColumTextAlignment("Garson", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Garson", "65*", "35*");
            report.AddTable("Garson", Resources.UserSales, "");

            foreach (var ownerInfo in owners)
            {
                report.AddRow("Garson", ownerInfo.UserName, ownerInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            var menuGroups = MenuGroupBuilder.CalculateMenuGroups(ReportContext.Tickets, ReportContext.MenuItems);

            report.AddColumTextAlignment("Gıda", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("Gıda", "45*", "Auto", "35*");
            report.AddTable("Gıda", Resources.ItemSales, "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("Gıda", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.Rate),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("Gıda", Resources.Total.ToUpper(), "", menuGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            return report.Document;
        }

        private static string GetPercent(int paymentType, IEnumerable<TenderedAmount> data)
        {
            var total = data.Sum(x => x.Amount);
            return total > 0 ? string.Format("%{0:0.00}", (GetAmount(paymentType, data) * 100) / total) : "%0";
        }

        private static decimal GetAmount(int paymentType, IEnumerable<TenderedAmount> data)
        {
            var r = data.SingleOrDefault(x => x.PaymentType == paymentType);
            return r != null ? r.Amount : 0;
        }

        protected override string GetHeader()
        {
            return Resources.WorkPeriodReport;
        }
    }
}
