using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public static class TicketFormatter
    {
        private static readonly IDepartmentService DepartmentService = ServiceLocator.Current.GetInstance<IDepartmentService>();
        private static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        private static ISettingReplacer _settingReplacer;

        public static string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate template)
        {
            _settingReplacer = SettingService.GetSettingReplacer();
            if (template.MergeLines) lines = MergeLines(lines);
            var orderNo = lines.Count() > 0 ? lines.ElementAt(0).OrderNumber : 0;
            var userNo = lines.Count() > 0 ? lines.ElementAt(0).CreatingUserName : "";
            var header = ReplaceDocumentVars(template.HeaderTemplate, ticket, orderNo, userNo);
            var footer = ReplaceDocumentVars(template.FooterTemplate, ticket, orderNo, userNo);
            var lns = lines.SelectMany(x => FormatLines(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            var result = header.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            result.AddRange(lns);
            result.AddRange(footer.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            return result.ToArray();
        }

        private static IEnumerable<Order> MergeLines(IEnumerable<Order> lines)
        {
            var group = lines.Where(x => x.OrderTagValues.Count == 0).GroupBy(x => new
                                                {
                                                    x.MenuItemId,
                                                    x.MenuItemName,
                                                    x.CalculatePrice,
                                                    DecreaseFromInventory = x.DecreaseInventory,
                                                    x.Price,
                                                    x.TaxAmount,
                                                    x.TaxTemplateId,
                                                    x.TaxIncluded,
                                                    x.PortionName,
                                                    x.PortionCount
                                                });

            var result = group.Select(x => new Order
                                    {
                                        MenuItemId = x.Key.MenuItemId,
                                        MenuItemName = x.Key.MenuItemName,
                                        CalculatePrice = x.Key.CalculatePrice,
                                        DecreaseInventory = x.Key.DecreaseFromInventory,
                                        Price = x.Key.Price,
                                        TaxAmount = x.Key.TaxAmount,
                                        TaxTemplateId = x.Key.TaxTemplateId,
                                        TaxIncluded = x.Key.TaxIncluded,
                                        CreatedDateTime = x.Last().CreatedDateTime,
                                        CreatingUserName = x.Last().CreatingUserName,
                                        OrderNumber = x.Last().OrderNumber,
                                        TicketId = x.Last().TicketId,
                                        PortionName = x.Key.PortionName,
                                        PortionCount = x.Key.PortionCount,
                                        Quantity = x.Sum(y => y.Quantity)
                                    });

            result = result.Union(lines.Where(x => x.OrderTagValues.Count > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }

        private static string ReplaceDocumentVars(string document, Ticket ticket, int orderNo, string userName)
        {
            string result = document;
            if (string.IsNullOrEmpty(document)) return "";

            result = FormatData(result, TagNames.TicketDate, () => ticket.Date.ToShortDateString());
            result = FormatData(result, TagNames.TicketTime, () => ticket.Date.ToShortTimeString());
            result = FormatData(result, TagNames.Date, () => DateTime.Now.ToShortDateString());
            result = FormatData(result, TagNames.Time, () => DateTime.Now.ToShortTimeString());
            result = FormatData(result, TagNames.TicketId, () => ticket.Id.ToString());
            result = FormatData(result, TagNames.TicketNo, () => ticket.TicketNumber);
            result = FormatData(result, TagNames.OrderNo, orderNo.ToString);
            result = FormatData(result, TagNames.TicketTag, ticket.GetTagData);
            result = FormatDataIf(true, result, TagNames.Department, () => GetDepartmentName(ticket.DepartmentId));

            const string ticketTagPattern = TagNames.TicketTag2 + "[^}]+}";

            while (Regex.IsMatch(result, ticketTagPattern))
            {
                var value = Regex.Match(result, ticketTagPattern).Groups[0].Value;
                var tags = "";
                try
                {
                    var tag = value.Trim('{', '}').Split(':')[1];
                    tags = tag.Split(',').Aggregate(tags, (current, t) => current +
                        (!string.IsNullOrEmpty(ticket.GetTagValue(t.Trim()))
                        ? (t + ": " + ticket.GetTagValue(t.Trim()) + "\r")
                        : ""));
                    result = FormatData(result.Trim('\r'), value, () => tags);
                }
                catch (Exception)
                {
                    result = FormatData(result, value, () => "");
                }
            }

            const string ticketTag2Pattern = TagNames.TicketTag3 + "[^}]+}";

            while (Regex.IsMatch(result, ticketTag2Pattern))
            {
                var value = Regex.Match(result, ticketTag2Pattern).Groups[0].Value;
                var tag = value.Trim('{', '}').Split(':')[1];
                var tagValue = ticket.GetTagValue(tag);
                try
                {
                    result = FormatData(result, value, () => tagValue);
                }
                catch (Exception)
                {
                    result = FormatData(result, value, () => "");
                }
            }

            var title = ticket.LocationName;
            if (string.IsNullOrEmpty(ticket.LocationName))
                title = userName;

            result = FormatData(result, TagNames.LocationUser, () => title);
            result = FormatData(result, TagNames.UserName, () => userName);
            result = FormatData(result, TagNames.Location, () => ticket.LocationName);
            result = FormatData(result, TagNames.Note, () => ticket.Note);
            result = FormatData(result, TagNames.AccName, () => ticket.AccountName);

            if (ticket.AccountId > 0 && (result.Contains(TagNames.AccAddress) || result.Contains(TagNames.AccPhone)))
            {
                var account = Dao.SingleWithCache<Account>(x => x.Id == ticket.AccountId);
                result = FormatData(result, TagNames.AccPhone, () => account.SearchString);
            }

            result = RemoveTag(result, TagNames.AccAddress);
            result = RemoveTag(result, TagNames.AccPhone);

            var payment = ticket.GetPaymentAmount();
            var remaining = ticket.GetRemainingAmount();
            var plainTotal = ticket.GetPlainSum();
            var preTaxServices = ticket.GetPreTaxServicesTotal();
            var taxAmount = ticket.CalculateTax(plainTotal, preTaxServices);  //GetTaxTotal(ticket.Orders, plainTotal, ticket.GetDiscountTotal());
            var servicesTotal = ticket.GetPostTaxServicesTotal();
            //ticket.CalculateTax(plainTotal, preTaxServices);

            result = FormatDataIf(taxAmount > 0 || preTaxServices > 0 || servicesTotal > 0, result, TagNames.PlainTotal, () => plainTotal.ToString("#,#0.00"));
            result = FormatDataIf(preTaxServices > 0, result, TagNames.DiscountTotal, () => preTaxServices.ToString("#,#0.00"));
            result = FormatDataIf(taxAmount > 0, result, TagNames.TaxTotal, () => taxAmount.ToString("#,#0.00"));
            result = FormatDataIf(taxAmount > 0, result, TagNames.TaxDetails, () => GetTaxDetails(ticket.Orders, plainTotal, preTaxServices));
            result = FormatDataIf(servicesTotal > 0, result, TagNames.CalculationDetails, () => GetServiceDetails(ticket));

            result = FormatDataIf(payment > 0, result, TagNames.IfPaid,
                () => string.Format(Resources.RemainingAmountIfPaidValue_f, payment.ToString("#,#0.00"), remaining.ToString("#,#0.00")));

            result = FormatDataIf(preTaxServices > 0, result, TagNames.IfDiscount,
                () => string.Format(Resources.DiscountTotalAndTicketTotalValue_f, (plainTotal).ToString("#,#0.00"), preTaxServices.ToString("#,#0.00")));

            result = FormatDataIf(preTaxServices < 0, result, TagNames.IfFlatten, () => string.Format(Resources.IfNegativeDiscountValue_f, preTaxServices.ToString("#,#0.00")));

            result = FormatData(result, TagNames.TicketTotal, () => ticket.GetSum().ToString("#,#0.00"));
            result = FormatData(result, TagNames.PaymentTotal, () => ticket.GetPaymentAmount().ToString("#,#0.00"));
            result = FormatData(result, TagNames.Balance, () => ticket.GetRemainingAmount().ToString("#,#0.00"));

            result = FormatData(result, TagNames.TotalText, () => HumanFriendlyInteger.CurrencyToWritten(ticket.GetSum()));
            result = FormatData(result, TagNames.Totaltext, () => HumanFriendlyInteger.CurrencyToWritten(ticket.GetSum(), true));

            result = _settingReplacer.ReplaceSettingValue("{SETTING:([^}]+)}", result);

            return result;
        }

        private static string GetDepartmentName(int departmentId)
        {
            var dep = DepartmentService.GetDepartment(departmentId);
            return dep != null ? dep.Name : Resources.UndefinedWithBrackets;
        }

        private static string GetServiceDetails(Ticket ticket)
        {
            var sb = new StringBuilder();
            foreach (var service in ticket.Calculations)
            {
                var lservice = service;
                var ts = SettingService.GetCalculationTemplateById(lservice.ServiceId);
                var tsTitle = ts != null ? ts.Name : Resources.UndefinedWithBrackets;
                sb.AppendLine("<J>" + tsTitle + ":|" + lservice.CalculationAmount.ToString("#,#0.00"));
            }
            return string.Join("\r", sb);
        }

        private static string GetTaxDetails(IEnumerable<Order> orders, decimal plainSum, decimal discount)
        {
            var sb = new StringBuilder();
            var groups = orders.Where(x => x.TaxTemplateId > 0).GroupBy(x => x.TaxTemplateId);
            foreach (var @group in groups)
            {
                var iGroup = @group;
                var tb = SettingService.GetTaxTemplateById(iGroup.Key);
                var tbTitle = tb != null ? tb.Name : Resources.UndefinedWithBrackets;
                var total = @group.Sum(x => x.TaxAmount * x.Quantity);
                if (discount > 0)
                {
                    total -= (total * discount) / plainSum;
                }
                if (total > 0) sb.AppendLine("<J>" + tbTitle + ":|" + total.ToString("#,#0.00"));
            }
            return string.Join("\r", sb);
        }

        private static decimal GetTaxTotal(IEnumerable<Order> orders, decimal plainSum, decimal discount)
        {
            var result = orders.Sum(x => x.GetTotalTaxAmount());
            if (discount > 0)
            {
                result -= (result * discount) / plainSum;
            }
            return result;
        }

        private static string FormatData(string data, string tag, Func<string> valueFunc)
        {
            if (!data.Contains(tag)) return data;

            var value = valueFunc.Invoke();
            var tagData = new TagData(data, tag);
            if (!string.IsNullOrEmpty(value)) value =
                !string.IsNullOrEmpty(tagData.Title) && tagData.Title.Contains("<value>")
                ? tagData.Title.Replace("<value>", value)
                : tagData.Title + value;
            return data.Replace(tagData.DataString, value);
        }

        private static string FormatDataIf(bool condition, string data, string tag, Func<string> valueFunc)
        {
            if (condition && data.Contains(tag)) return FormatData(data, tag, valueFunc.Invoke);
            return RemoveTag(data, tag);
        }

        private static string RemoveTag(string data, string tag)
        {
            var tagData = new TagData(data, tag);
            return data.Replace(tagData.DataString, "");
        }

        private static string FormatLines(PrinterTemplate template, Order order)
        {
            if (!string.IsNullOrEmpty(template.LineTemplate))
                return template.LineTemplate.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Aggregate("", (current, s) => current + ReplaceLineVars(s, order));
            return "";
        }

        private static string ReplaceLineVars(string line, Order order)
        {
            string result = line;

            if (order != null)
            {
                result = FormatData(result, TagNames.Quantity, () => order.Quantity.ToString("#,#0.##"));
                result = FormatData(result, TagNames.Name, () => order.MenuItemName + order.GetPortionDesc());
                result = FormatData(result, TagNames.Price, () => order.Price.ToString("#,#0.00"));
                result = FormatData(result, TagNames.Total, () => order.GetItemPrice().ToString("#,#0.00"));
                result = FormatData(result, TagNames.TotalAmount, () => order.GetItemValue().ToString("#,#0.00"));
                result = FormatData(result, TagNames.Cents, () => (order.Price * 100).ToString("#,##"));
                result = FormatData(result, TagNames.LineAmount, () => order.GetTotal().ToString("#,#0.00"));
                result = FormatData(result, TagNames.OrderNo, () => order.OrderNumber.ToString());
                result = FormatData(result, TagNames.PriceTag, () => order.PriceTag);
                result = _settingReplacer.ReplaceSettingValue("{SETTING:([^}]+)}", result);

                if (result.Contains(TagNames.Properties.Substring(0, TagNames.Properties.Length - 1)))
                {
                    string lineFormat = result;
                    if (order.OrderTagValues.Count > 0)
                    {
                        string label = "";
                        foreach (var property in order.OrderTagValues)
                        {
                            var itemProperty = property;
                            var lineValue = FormatData(lineFormat, TagNames.Properties, () => itemProperty.Name);
                            lineValue = FormatData(lineValue, TagNames.PropQuantity, () => itemProperty.Quantity.ToString("#.##"));
                            lineValue = FormatData(lineValue, TagNames.PropPrice, () => itemProperty.AddTagPriceToOrderPrice ? "" : itemProperty.Price.ToString("#,#0.00"));
                            label += lineValue + "\r\n";
                        }
                        result = "\r\n" + label;
                    }
                    else result = "";
                }
                result = result.Replace("<", "\r\n<");
            }
            return result;
        }
    }

    public static class HumanFriendlyInteger
    {
        static readonly string[] Ones = new[] { "", Resources.One, Resources.Two, Resources.Three, Resources.Four, Resources.Five, Resources.Six, Resources.Seven, Resources.Eight, Resources.Nine };
        static readonly string[] Teens = new[] { Resources.Ten, Resources.Eleven, Resources.Twelve, Resources.Thirteen, Resources.Fourteen, Resources.Fifteen, Resources.Sixteen, Resources.Seventeen, Resources.Eighteen, Resources.Nineteen };
        static readonly string[] Tens = new[] { Resources.Twenty, Resources.Thirty, Resources.Forty, Resources.Fifty, Resources.Sixty, Resources.Seventy, Resources.Eighty, Resources.Ninety };
        static readonly string[] ThousandsGroups = { "", " " + Resources.Thousand, " " + Resources.Million, " " + Resources.Billion };

        private static string FriendlyInteger(int n, string leftDigits, int thousands)
        {
            if (n == 0)
            {
                return leftDigits;
            }
            string friendlyInt = leftDigits;
            if (friendlyInt.Length > 0)
            {
                friendlyInt += " ";
            }
            if (n < 10)
            {
                friendlyInt += Ones[n];
            }
            else if (n < 20)
            {
                friendlyInt += Teens[n - 10];
            }
            else if (n < 100)
            {
                friendlyInt += FriendlyInteger(n % 10, Tens[n / 10 - 2], 0);
            }
            else if (n < 1000)
            {
                var t = Ones[n / 100] + " " + Resources.Hundred;
                if (n / 100 == 1) t = Resources.OneHundred;
                friendlyInt += FriendlyInteger(n % 100, t, 0);
            }
            else if (n < 10000 && thousands == 0)
            {
                var t = Ones[n / 1000] + " " + Resources.Thousand;
                if (n / 1000 == 1) t = Resources.OneThousand;
                friendlyInt += FriendlyInteger(n % 1000, t, 0);
            }
            else
            {
                friendlyInt += FriendlyInteger(n % 1000, FriendlyInteger(n / 1000, "", thousands + 1), 0);
            }

            return friendlyInt + ThousandsGroups[thousands];
        }

        public static string CurrencyToWritten(decimal d, bool upper = false)
        {
            var result = "";
            var fraction = d - Math.Floor(d);
            var value = d - fraction;
            if (value > 0)
            {
                var start = IntegerToWritten(Convert.ToInt32(value));
                if (upper) start = start.Replace(" ", "").ToUpper();
                result += string.Format("{0} {1} ", start, Resources.Dollar + GetPlural(value));
            }

            if (fraction > 0)
            {
                var end = IntegerToWritten(Convert.ToInt32(fraction * 100));
                if (upper) end = end.Replace(" ", "").ToUpper();
                result += string.Format("{0} {1} ", end, Resources.Cent + GetPlural(fraction));
            }
            return result.Replace("  ", " ").Trim();
        }

        private static string GetPlural(decimal number)
        {
            return number == 1 ? "" : Resources.PluralCurrencySuffix;
        }

        public static string IntegerToWritten(int n)
        {
            if (n == 0)
            {
                return Resources.Zero;
            }
            if (n < 0)
            {
                return Resources.Negative + " " + IntegerToWritten(-n);
            }
            return FriendlyInteger(n, "", 0);
        }
    }
}
