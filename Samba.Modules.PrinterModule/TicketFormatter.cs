using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Modules.PrinterModule
{
    public static class TicketFormatter
    {
        private static readonly IDepartmentService DepartmentService =
            ServiceLocator.Current.GetInstance(typeof(IDepartmentService)) as IDepartmentService;
        private static readonly IUserService UserService =
            ServiceLocator.Current.GetInstance(typeof(IUserService)) as IUserService;

        public static string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate template)
        {
            if (template.MergeLines) lines = MergeLines(lines);
            var orderNo = lines.Count() > 0 ? lines.ElementAt(0).OrderNumber : 0;
            var userNo = lines.Count() > 0 ? lines.ElementAt(0).CreatingUserId : 0;
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
                                        CreatingUserId = x.Last().CreatingUserId,
                                        OrderNumber = x.Last().OrderNumber,
                                        TicketId = x.Last().TicketId,
                                        PortionName = x.Key.PortionName,
                                        PortionCount = x.Key.PortionCount,
                                        Quantity = x.Sum(y => y.Quantity)
                                    });

            result = result.Union(lines.Where(x => x.OrderTagValues.Count > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }

        private static string ReplaceDocumentVars(string document, Ticket ticket, int orderNo, int userNo)
        {
            string result = document;
            if (string.IsNullOrEmpty(document)) return "";

            result = FormatData(result, Resources.TF_TicketDate, () => ticket.Date.ToShortDateString());
            result = FormatData(result, Resources.TF_TicketTime, () => ticket.Date.ToShortTimeString());
            result = FormatData(result, Resources.TF_DayDate, () => DateTime.Now.ToShortDateString());
            result = FormatData(result, Resources.TF_DayTime, () => DateTime.Now.ToShortTimeString());
            result = FormatData(result, Resources.TF_UniqueTicketId, () => ticket.Id.ToString());
            result = FormatData(result, Resources.TF_TicketNumber, () => ticket.TicketNumber);
            result = FormatData(result, Resources.TF_LineOrderNumber, orderNo.ToString);
            result = FormatData(result, Resources.TF_TicketTag, ticket.GetTagData);
            result = FormatDataIf(true, result, "{DEPARTMENT}", () => GetDepartmentName(ticket.DepartmentId));

            if (result.Contains(Resources.TF_OptionalTicketTag))
            {
                var start = result.IndexOf(Resources.TF_OptionalTicketTag);
                var end = result.IndexOf("}", start) + 1;
                var value = result.Substring(start, end - start);
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

            var userName = UserService.GetUserName(userNo);

            var title = ticket.LocationName;
            if (string.IsNullOrEmpty(ticket.LocationName))
                title = userName;

            result = FormatData(result, Resources.TF_LocationOrUserName, () => title);
            result = FormatData(result, Resources.TF_UserName, () => userName);
            result = FormatData(result, Resources.TF_LocationName, () => ticket.LocationName);
            result = FormatData(result, Resources.TF_TicketNote, () => ticket.Note);
            result = FormatData(result, Resources.TF_AccountName, () => ticket.AccountName);

            if (ticket.AccountId > 0 && (result.Contains(Resources.TF_AccountAddress) || result.Contains(Resources.TF_AccountPhone)))
            {
                var account = Dao.SingleWithCache<Account>(x => x.Id == ticket.AccountId);
                result = FormatData(result, Resources.TF_AccountPhone, () => account.SearchString);
            }

            result = RemoveTag(result, Resources.TF_AccountAddress);
            result = RemoveTag(result, Resources.TF_AccountPhone);

            var payment = ticket.GetPaymentAmount();
            var remaining = ticket.GetRemainingAmount();
            var discount = ticket.GetDiscountAndRoundingTotal();
            var plainTotal = ticket.GetPlainSum();
            var taxAmount = ticket.CalculateTax();
            var servicesTotal = ticket.GetServicesTotal();

            result = FormatDataIf(taxAmount > 0 || discount > 0 || servicesTotal > 0, result, "{PLAIN TOTAL}", () => plainTotal.ToString("#,#0.00"));
            result = FormatDataIf(discount > 0, result, "{DISCOUNT TOTAL}", () => discount.ToString("#,#0.00"));
            result = FormatDataIf(taxAmount > 0, result, "{TAX TOTAL}", () => taxAmount.ToString("#,#0.00"));
            result = FormatDataIf(taxAmount > 0, result, "{SERVICE TOTAL}", () => servicesTotal.ToString("#,#0.00"));
            result = FormatDataIf(taxAmount > 0, result, "{TAX DETAILS}", () => GetTaxDetails(ticket.Orders, plainTotal, discount));
            result = FormatDataIf(servicesTotal > 0, result, "{SERVICE DETAILS}", () => GetServiceDetails(ticket));

            result = FormatDataIf(payment > 0, result, Resources.TF_RemainingAmountIfPaid,
                () => string.Format(Resources.RemainingAmountIfPaidValue_f, payment.ToString("#,#0.00"), remaining.ToString("#,#0.00")));

            result = FormatDataIf(discount > 0, result, Resources.TF_DiscountTotalAndTicketTotal,
                () => string.Format(Resources.DiscountTotalAndTicketTotalValue_f, (plainTotal).ToString("#,#0.00"), discount.ToString("#,#0.00")));

            result = FormatDataIf(discount < 0, result, Resources.TF_IfFlatten, () => string.Format(Resources.IfNegativeDiscountValue_f, discount.ToString("#,#0.00")));

            result = FormatData(result, Resources.TF_TicketTotal, () => ticket.GetSum().ToString("#,#0.00"));
            result = FormatData(result, Resources.TF_TicketPaidTotal, () => ticket.GetPaymentAmount().ToString("#,#0.00"));
            result = FormatData(result, Resources.TF_TicketRemainingAmount, () => ticket.GetRemainingAmount().ToString("#,#0.00"));

            result = FormatData(result, "{TOTAL TEXT}", () => HumanFriendlyInteger.CurrencyToWritten(ticket.GetSum()));
            result = FormatData(result, "{TOTALTEXT}", () => HumanFriendlyInteger.CurrencyToWritten(ticket.GetSum(), true));

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
            foreach (var service in ticket.Services)
            {
                var lservice = service;
                var ts = AppServices.MainDataContext.ServiceTemplates.FirstOrDefault(x => x.Id == lservice.ServiceId);
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
                var tb = AppServices.MainDataContext.TaxTemplates.FirstOrDefault(x => x.Id == iGroup.Key);
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
                result = FormatData(result, Resources.TF_LineItemQuantity, () => order.Quantity.ToString("#,#0.##"));
                result = FormatData(result, Resources.TF_LineItemName, () => order.MenuItemName + order.GetPortionDesc());
                result = FormatData(result, Resources.TF_LineItemPrice, () => order.Price.ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineItemTotal, () => order.GetItemPrice().ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineItemTotalAndQuantity, () => order.GetItemValue().ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineItemPriceCents, () => (order.Price * 100).ToString("#,##"));
                result = FormatData(result, Resources.TF_LineItemTotalWithoutGifts, () => order.GetTotal().ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineOrderNumber, () => order.OrderNumber.ToString());
                result = FormatData(result, "{PRICE TAG}", () => order.PriceTag);
                if (result.Contains(Resources.TF_LineItemDetails.Substring(0, Resources.TF_LineItemDetails.Length - 1)))
                {
                    string lineFormat = result;
                    if (order.OrderTagValues.Count > 0)
                    {
                        string label = "";
                        foreach (var property in order.OrderTagValues)
                        {
                            var itemProperty = property;
                            var lineValue = FormatData(lineFormat, Resources.TF_LineItemDetails, () => itemProperty.Name);
                            lineValue = FormatData(lineValue, Resources.TF_LineItemDetailQuantity, () => itemProperty.Quantity.ToString("#.##"));
                            lineValue = FormatData(lineValue, Resources.TF_LineItemDetailPrice, () => itemProperty.AddTagPriceToOrderPrice ? "" : itemProperty.Price.ToString("#,#0.00"));
                            label += lineValue + "\r\n";
                        }
                        result = "\r\n" + label;
                    }
                    else result = "";
                }
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
