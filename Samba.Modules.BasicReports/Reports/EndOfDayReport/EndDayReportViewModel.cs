using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.BasicReports.Reports.EndOfDayReport
{
    public class EndDayReportViewModel : ReportViewModelBase
    {
        private readonly ICacheService _cacheService;

        public EndDayReportViewModel(IUserService userService, IApplicationState applicationState, ILogService logService, ISettingService settingService, ICacheService cacheService)
            : base(userService, applicationState, logService, settingService)
        {
            _cacheService = cacheService;
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
            AddDefaultReportHeader(report, currentPeriod, Resources.WorkPeriodReport);

            //---------------

            CreateTicketTypeInfo(report, ReportContext.Tickets.Where(x => x.TotalAmount >= 0), Resources.Sales);
            var refundTickets = ReportContext.Tickets.Where(x => x.TotalAmount < 0).ToList();
            if (refundTickets.Any()) CreateTicketTypeInfo(report, refundTickets, "Returns");

            //---------------

            var incomeCalculator = ReportContext.GetIncomeCalculator();

            report.AddColumnLength("GelirlerTablosu", "45*", "Auto", "35*");
            report.AddColumTextAlignment("GelirlerTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddTable("GelirlerTablosu", Resources.Incomes, "", "");

            foreach (var paymentName in incomeCalculator.PaymentNames)
                report.AddRow("GelirlerTablosu", paymentName, incomeCalculator.GetPercent(paymentName), incomeCalculator.GetAmount(paymentName).ToString(ReportContext.CurrencyFormat));

            report.AddRow("GelirlerTablosu", Resources.TotalIncome.ToUpper(), "", incomeCalculator.TotalAmount.ToString(ReportContext.CurrencyFormat));

            //---------------

            var refundCalculator = ReportContext.GetRefundCalculator();
            if (refundCalculator.TotalAmount != 0)
            {
                report.AddColumnLength("İadeTablosu", "45*", "Auto", "35*");
                report.AddColumTextAlignment("İadeTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddTable("İadeTablosu", "Refunds", "", "");

                foreach (var paymentName in refundCalculator.PaymentNames)
                    report.AddRow("İadeTablosu", paymentName, refundCalculator.GetPercent(paymentName),
                                  refundCalculator.GetAmount(paymentName).ToString(ReportContext.CurrencyFormat));

                report.AddRow("İadeTablosu", "TOTAL REFUND", "", 
                    refundCalculator.TotalAmount.ToString(ReportContext.CurrencyFormat));
            }

            //---------------

            var ticketGropus = ReportContext.Tickets
                .GroupBy(x => new { x.TicketTypeId })
                .Select(x => new TicketTypeInfo
                {
                    TicketTypeId = x.Key.TicketTypeId,
                    TicketCount = x.Count(),
                    Amount = x.Sum(y => y.GetSum()),
                    Tax = x.Sum(y => y.CalculateTax(y.GetPlainSum(), y.GetPreTaxServicesTotal())),
                    Services = x.Sum(y => y.GetPostTaxServicesTotal())
                }).ToList();

            var propertySum = ReportContext.Tickets
                .SelectMany(x => x.Orders)
                .Sum(x => x.GetOrderTagPrice() * x.Quantity);

            var discounts = Math.Abs(ReportContext.Tickets.Sum(x => x.GetPreTaxServicesTotal()));

            report.AddColumTextAlignment("Bilgi", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Bilgi", "65*", "35*");
            report.AddTable("Bilgi", Resources.GeneralInformation, "");
            report.AddRow("Bilgi", Resources.ItemProperties, propertySum.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", Resources.DiscountsTotal, discounts.ToString(ReportContext.CurrencyFormat));

            if (ticketGropus.Count() > 1)
            {
                foreach (var ticketTypeInfo in ticketGropus)
                {
                    report.AddRow("Bilgi", ticketTypeInfo.TicketTypeName, ticketTypeInfo.TicketCount.ToString());
                }
            }

            report.AddBoldRow("Bilgi", Resources.Orders, "");

            var orderCount = ReportContext.Tickets.Sum(x => x.Orders.Count);

            report.AddRow("Bilgi", Resources.OrderCount, orderCount.ToString());

            var orderStates = ReportContext.Tickets
                   .SelectMany(x => x.Orders)
                   .SelectMany(x => x.GetOrderStateValues()).Distinct().ToList();

            if (orderStates.Any())
            {
                foreach (var orderStateValue in orderStates.Where(x => _cacheService.CanShowStateOnEndOfDayReport(x.StateName, x.State)).OrderBy(x => x.OrderKey).ThenBy(x => x.StateValue))
                {
                    var value = orderStateValue;
                    var items = ReportContext.Tickets.SelectMany(x => x.Orders).Where(x => x.IsInState(value.StateName, value.State, value.StateValue)).ToList();
                    var amount = items.Sum(x => x.GetValue());
                    var count = items.Count();
                    report.AddRow("Bilgi", string.Format("{0} {1} ({2})", orderStateValue.State, orderStateValue.StateValue, count), amount.ToString(ReportContext.CurrencyFormat));
                }
            }

            var ticketStates = ReportContext.Tickets
                .SelectMany(x => x.GetTicketStateValues()).Distinct().ToList();

            report.AddBoldRow("Bilgi", Resources.Tickets, "");

            if (ticketStates.Any())
            {
                foreach (var ticketStateValue in ticketStates.Where(x => _cacheService.CanShowStateOnEndOfDayReport(x.StateName, x.State)))
                {
                    TicketStateValue value = ticketStateValue;
                    var items = ReportContext.Tickets.Where(x => x.IsInState(value.StateName, value.State)).ToList();
                    var amount = items.Sum(x => x.GetSum());
                    var count = items.Count();
                    report.AddRow("Bilgi", string.Format("{0} ({1})", ticketStateValue.State, count), amount.ToString(ReportContext.CurrencyFormat));
                }
            }

            var ticketCount = ticketGropus.Sum(x => x.TicketCount);

            report.AddRow("Bilgi", Resources.TicketCount, ticketCount.ToString());

            report.AddRow("Bilgi", Resources.SalesDivTicket, ticketCount > 0
                ? (ticketGropus.Sum(x => x.Amount) / ticketGropus.Sum(x => x.TicketCount)).ToString(ReportContext.CurrencyFormat)
                : "0");

            if (ticketGropus.Count() > 1)
            {
                foreach (var ticketTypeInfo in ticketGropus)
                {
                    var dinfo = ticketTypeInfo;

                    var groups = ReportContext.Tickets
                        .Where(x => x.TicketTypeId == dinfo.TicketTypeId)
                        .SelectMany(x => x.Payments)
                        .GroupBy(x => new { x.Name })
                        .Select(x => new TenderedAmount { PaymentName = x.Key.Name, Amount = x.Sum(y => y.Amount) });

                    var ticketTypeAmountCalculator = new AmountCalculator(groups);

                    report.AddColumnLength(ticketTypeInfo.TicketTypeName + Resources.Incomes, "40*", "Auto", "35*");
                    report.AddColumTextAlignment(ticketTypeInfo.TicketTypeName + Resources.Incomes, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                    report.AddTable(ticketTypeInfo.TicketTypeName + Resources.Incomes, string.Format(Resources.Incomes_f, ticketTypeInfo.TicketTypeName), "", "");

                    foreach (var paymentName in ticketTypeAmountCalculator.PaymentNames)
                    {
                        report.AddRow(ticketTypeInfo.TicketTypeName + Resources.Incomes, paymentName, ticketTypeAmountCalculator.GetPercent(paymentName), ticketTypeAmountCalculator.GetAmount(paymentName).ToString(ReportContext.CurrencyFormat));
                    }

                    report.AddRow(ticketTypeInfo.TicketTypeName + Resources.Incomes, Resources.TotalIncome, "", ticketTypeInfo.Amount.ToString(ReportContext.CurrencyFormat));

                    var ddiscounts = ReportContext.Tickets
                        .Where(x => x.TicketTypeId == dinfo.TicketTypeId)
                        .Sum(x => x.GetPreTaxServicesTotal());

                    ddiscounts = Math.Abs(ddiscounts);

                    report.AddRow(ticketTypeInfo.TicketTypeName + Resources.Incomes, Resources.DiscountsTotal, "", ddiscounts.ToString(ReportContext.CurrencyFormat));

                    report.AddRow(ticketTypeInfo.TicketTypeName + Resources.Incomes, Resources.TaxAmount, "", ticketTypeInfo.Tax.ToString(ReportContext.CurrencyFormat));             
                
                }
            }

            //--

            if (ReportContext.Tickets.Select(x => x.GetTagData()).Where(x => !string.IsNullOrEmpty(x)).Distinct().Any())
            {
                var dict = new Dictionary<string, List<Ticket>>();

                foreach (var ticket in ReportContext.Tickets.Where(x => x.IsTagged))
                {
                    foreach (var tag in ticket.GetTicketTagValues().Select(x => x.TagName + ":" + x.TagValue))
                    {
                        if (!dict.ContainsKey(tag))
                            dict.Add(tag, new List<Ticket>());
                        dict[tag].Add(ticket);
                    }
                }

                var tagGroups = dict.Select(x => new TicketTagInfo { Amount = x.Value.Sum(y => y.GetPlainSum()), TicketCount = x.Value.Count, TagName = x.Key }).OrderBy(x => x.TagName);

                var tagGrp = tagGroups.GroupBy(x => x.TagName.Split(':')[0]).ToList();
                if (tagGrp.Any())
                {
                    report.AddColumTextAlignment("Etiket", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                    report.AddColumnLength("Etiket", "45*", "Auto", "35*");
                    report.AddTable("Etiket", Resources.TicketTag.ToPlural(), "", "");
                }

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
                        report.AddRow("Etiket", Resources.TicketCount, "", tCount.ToString());
                        report.AddRow("Etiket", Resources.TicketTotal, "", amnt.ToString(ReportContext.CurrencyFormat));
                        report.AddRow("Etiket", Resources.Rate, "", rate.ToString("%#0.##"));
                        continue;
                    }

                    foreach (var ticketTagInfo in grp)
                    {
                        report.AddRow("Etiket",
                            ticketTagInfo.TagName.Split(':')[1],
                            ticketTagInfo.TicketCount.ToString(),
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

            var owners = ReportContext.Tickets.SelectMany(ticket => ticket.Orders.Where(x => !x.IncreaseInventory).Select(order => new { Ticket = ticket, Order = order }))
                .GroupBy(x => new { x.Order.CreatingUserName })
                .Select(x => new UserInfo { UserName = x.Key.CreatingUserName, Amount = x.Sum(y => MenuGroupBuilder.CalculateOrderTotal(y.Ticket, y.Order)) }).ToList();

            if (owners.Any())
            {
                report.AddColumTextAlignment("Garson", TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("Garson", "65*", "35*");
                report.AddTable("Garson", Resources.UserSales, "");
            }

            foreach (var ownerInfo in owners)
            {
                report.AddRow("Garson", ownerInfo.UserName, ownerInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            //----

            var refundOwners = ReportContext.Tickets.SelectMany(ticket => ticket.Orders.Where(x => x.IncreaseInventory).Select(order => new { Ticket = ticket, Order = order }))
                .GroupBy(x => new { x.Order.CreatingUserName })
                .Select(x => new UserInfo { UserName = x.Key.CreatingUserName, Amount = x.Sum(y => MenuGroupBuilder.CalculateOrderTotal(y.Ticket, y.Order)) }).ToList();
            if (refundOwners.Any())
            {
                report.AddColumTextAlignment("Garsonİade", TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("Garsonİade", "65*", "35*");
                report.AddTable("Garsonİade", "User Returns", "");

                foreach (var ownerInfo in refundOwners)
                {
                    report.AddRow("Garsonİade", ownerInfo.UserName,
                                  ownerInfo.Amount.ToString(ReportContext.CurrencyFormat));
                }
            }

            var uInfo = ReportContext.Tickets.SelectMany(x => x.Payments).Select(x => x.UserId).Distinct().Select(x => new UserInfo { UserId = x, UserName = ReportContext.GetUserName(x) }).ToList();

            if (uInfo.Count() > 1)
            {
                foreach (var userInfo in uInfo)
                {
                    var userIncomeCalculator = ReportContext.GetIncomeCalculatorByUser(userInfo.UserId);

                    report.AddColumnLength(userInfo.UserName + Resources.Incomes, "40*", "Auto", "35*");
                    report.AddColumTextAlignment(userInfo.UserName + Resources.Incomes, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                    report.AddTable(userInfo.UserName + Resources.Incomes, string.Format(Resources.SettledBy_f, userInfo.UserName), "", "");

                    foreach (var paymentName in userIncomeCalculator.PaymentNames)
                        report.AddRow(userInfo.UserName + Resources.Incomes, paymentName, userIncomeCalculator.GetPercent(paymentName), userIncomeCalculator.GetAmount(paymentName).ToString(ReportContext.CurrencyFormat));

                    report.AddRow(userInfo.UserName + Resources.Incomes, Resources.TotalIncome.ToUpper(), "", userIncomeCalculator.TotalAmount.ToString(ReportContext.CurrencyFormat));
                }
            }


            var menuGroups = MenuGroupBuilder.CalculateMenuGroups(ReportContext.Tickets, ReportContext.MenuItems).ToList();

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

        private static void CreateTicketTypeInfo(SimpleReport report, IEnumerable<Ticket> tickets, string header)
        {
            var rpKey = "TicketType" + header;
            report.AddColumTextAlignment(rpKey, TextAlignment.Left, TextAlignment.Right);
            report.AddTable(rpKey, header, "");

            var ticketGropus = tickets
                .GroupBy(x => new { x.TicketTypeId })
                .Select(x => new TicketTypeInfo
                                 {
                                     TicketTypeId = x.Key.TicketTypeId,
                                     TicketCount = x.Count(),
                                     Discount = x.Sum(y => y.GetPreTaxServicesTotal()),
                                     Amount = x.Sum(y => y.GetSum()) - x.Sum(y => y.CalculateTax(y.GetPlainSum(), y.GetPreTaxServicesTotal())) - x.Sum(y => y.GetPostTaxServicesTotal()),
                                     Tax = x.Sum(y => y.CalculateTax(y.GetPlainSum(), y.GetPreTaxServicesTotal())),
                                     Services = x.Sum(y => y.GetPostTaxServicesTotal())
                                 }).ToList();

            if (ticketGropus.Count() > 1)
            {
                foreach (var ticketTypeInfo in ticketGropus)
                {
                    report.AddRow(rpKey, ticketTypeInfo.TicketTypeName,
                                  ticketTypeInfo.Amount.ToString(ReportContext.CurrencyFormat));
                }
            }

            var discountSum = ticketGropus.Sum(x => x.Discount);

            if (discountSum != 0)
            {
                report.AddRow(rpKey, string.Format(Resources.Total_f, header.ToUpper()).ToUpper(),
                              ticketGropus.Sum(x => x.Amount - x.Discount).ToString(ReportContext.CurrencyFormat));
                var services = ReportContext.Tickets.SelectMany(x => x.Calculations).Where(x => !x.IncludeTax).OrderBy(x => x.Order);
                services.GroupBy(x => x.CalculationTypeId).ToList().ForEach(
                    x =>
                    {
                        var template = ReportContext.CalculationTypes.SingleOrDefault(y => y.Id == x.Key);
                        var title = template != null ? "  " + template.Name : Resources.UndefinedWithBrackets;
                        report.AddRow(rpKey, title,
                                      x.Sum(y => y.CalculationAmount).ToString(ReportContext.CurrencyFormat));
                    });
            }

            report.AddRow(rpKey, string.Format(Resources.Total_f, header.ToUpper()).ToUpper(),
                                      ticketGropus.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));

            var taxSum = ticketGropus.Sum(x => x.Tax);
            var serviceSum = ticketGropus.Sum(x => x.Services);
            if (taxSum != 0 || serviceSum != 0)
            {
                if (serviceSum != 0)
                {
                    var services = ReportContext.Tickets.SelectMany(x => x.Calculations).Where(x => x.IncludeTax).OrderBy(x => x.Order);
                    services.GroupBy(x => x.CalculationTypeId).ToList().ForEach(
                        x =>
                        {
                            var template = ReportContext.CalculationTypes.SingleOrDefault(y => y.Id == x.Key);
                            var title = template != null ? "  " + template.Name : Resources.UndefinedWithBrackets;
                            report.AddRow(rpKey, title,
                                          x.Sum(y => y.CalculationAmount).ToString(ReportContext.CurrencyFormat));
                        });
                }

                if (taxSum != 0)
                {
                    report.AddRow(rpKey, Resources.SubTotal.ToUpper(),
                                  ticketGropus.Sum(x => x.Amount + x.Services).ToString(ReportContext.CurrencyFormat));

                    if (ReportContext.TaxTemplates.Count() > 1)
                    {
                        foreach (var taxTemplate in ReportContext.TaxTemplates)
                        {
                            if (taxTemplate.AccountTransactionType != null)
                            {
                                var tax = ReportContext.Tickets.Sum(x => x.GetTaxTotal(taxTemplate.AccountTransactionType.Id, x.GetPreTaxServicesTotal(), x.GetPlainSum()));
                                report.AddRow(rpKey, taxTemplate.Name, tax.ToString(ReportContext.CurrencyFormat));
                            }
                        }
                    }

                    report.AddRow(rpKey, Resources.TaxTotal.ToUpper(), taxSum.ToString(ReportContext.CurrencyFormat));
                }

                report.AddRow(rpKey, Resources.GrandTotal.ToUpper(),
                              ticketGropus.Sum(x => x.Amount + x.Tax + x.Services).ToString(ReportContext.CurrencyFormat));
            }
        }

        protected override string GetHeader()
        {
            return Resources.WorkPeriodReport;
        }
    }
}
