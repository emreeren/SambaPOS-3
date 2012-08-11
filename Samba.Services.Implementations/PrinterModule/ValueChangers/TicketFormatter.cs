using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
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
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();
        private static readonly IAccountService AccountService = ServiceLocator.Current.GetInstance<IAccountService>();

        public static string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate template)
        {
            var orders = lines.ToList();

            if (template.MergeLines) orders = MergeLines(orders).ToList();
            var orderNo = orders.Count() > 0 ? orders.ElementAt(0).OrderNumber : 0;
            var userNo = orders.Count() > 0 ? orders.ElementAt(0).CreatingUserName : "";

            string content = FormatLayout(template, ticket, orderNo, userNo);
            content = FormatData(content, "{RESOURCES}", () => string.Join("\r\n", ticket.TicketResources.SelectMany(x => FormatResource(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))));
            content = FormatData(content, "{ORDERS}", () => string.Join("\r\n", orders.SelectMany(x => FormatOrder(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))));

            content = SettingService.GetSettingReplacer().ReplaceSettingValue("{SETTING:([^}]+)}", content);

            return content.Split(new[] { '\r', '\n' }).ToArray();
        }

        private static string FormatResource(PrinterTemplate template, TicketResource ticketResource)
        {
            var resourceTemplate = CacheService.GetResourceTemplateById(ticketResource.ResourceTemplateId);
            if (resourceTemplate == null) return "";

            var templateName = "RESOURCES" + (!string.IsNullOrEmpty(resourceTemplate.Name) ? ":" + resourceTemplate.Name : "");
            var templatePart = template.GetPart(templateName);
            if (!string.IsNullOrEmpty(templatePart))
                return ReplaceResourceValues(templatePart, ticketResource);
            return "";
        }

        private static string ReplaceResourceValues(string templatePart, TicketResource ticketResource)
        {

            var result = templatePart;
            if (ticketResource != null)
            {
                result = FormatData(result, "{RESOURCE NAME}", () => ticketResource.ResourceName);
                result = FormatDataIf(ticketResource.AccountId > 0, result, "{RESOURCE BALANCE}", () => AccountService.GetAccountBalance(ticketResource.AccountId).ToString("#,#0.00"));
                if (result.Contains("{RESOURCE DATA:"))
                {
                    const string resourceDataPattern = "{RESOURCE DATA:" + "[^}]+}";
                    var resource = CacheService.GetResourceById(ticketResource.ResourceId);
                    while (Regex.IsMatch(result, resourceDataPattern))
                    {
                        var value = Regex.Match(result, resourceDataPattern).Groups[0].Value;
                        try
                        {
                            var tag = value.Trim('{', '}').Split(':')[1];
                            result = FormatData(result.Trim('\r'), value, () => string.Join("\r", tag.Split(',').Select(x => resource.GetCustomDataFormat(x, x + ": {0}"))));
                        }
                        catch (Exception)
                        {
                            result = FormatData(result, value, () => "");
                        }
                    }
                }
                return result;
            }
            return "";
        }

        private static string FormatOrder(PrinterTemplate template, Order order)
        {
            var templateName = "ORDERS" + (!string.IsNullOrEmpty(order.OrderStateGroupName) ? ":" + order.OrderStateGroupName : "");
            var templatePart = template.GetPart(templateName);
            if (!string.IsNullOrEmpty(templatePart))
                return ReplaceOrderValues(templatePart, order, template);
            return "";
        }

        private static string ReplaceOrderValues(string orderTemplate, Order order, PrinterTemplate template)
        {
            string result = orderTemplate;

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
                result = FormatData(result, "{ORDER TAGS}", () => string.Join("\r\n", order.OrderTagValues.SelectMany(x => FormatOrderTagValue(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))));
            }
            return result;
        }

        private static string FormatOrderTagValue(PrinterTemplate template, OrderTagValue orderTagValue)
        {
            var templateName = "ORDER TAGS" + (!string.IsNullOrEmpty(orderTagValue.Name) ? ":" + orderTagValue.Name : "");
            var templatePart = template.GetPart(templateName);
            if (!string.IsNullOrEmpty(templatePart))
            {
                return ReplaceOrderTagValues(templatePart, orderTagValue);
            }
            return "";
        }

        private static string ReplaceOrderTagValues(string templatePart, OrderTagValue value)
        {
            var otResult = templatePart;
            otResult = FormatDataIf(value.Price != 0, otResult, TagNames.OrderTagPrice, () => value.AddTagPriceToOrderPrice ? "" : value.Price.ToString("#,#0.00"));
            otResult = FormatDataIf(value.Quantity != 0, otResult, TagNames.OrderTagQuantity, () => value.Quantity.ToString("#.##"));
            otResult = FormatDataIf(!string.IsNullOrEmpty(value.Name), otResult, TagNames.OrderTagName, () => value.Name);
            return otResult;
        }

        private static string FormatLayout(PrinterTemplate template, Ticket ticket, int orderNo, string userName)
        {
            string result = template.Layout;
            if (string.IsNullOrEmpty(result)) return "";

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

            var title = userName;

            result = FormatData(result, TagNames.LocationUser, () => title);
            result = FormatData(result, TagNames.UserName, () => userName);
            result = FormatData(result, TagNames.Note, () => ticket.Note);
            result = FormatData(result, TagNames.AccName, () => ticket.AccountName);


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

            return result;
        }

        private static IEnumerable<Order> MergeLines(IEnumerable<Order> lines)
        {
            var group = lines.Where(x => x.OrderTagValues.Count == 0).GroupBy(x => new
                                                {
                                                    x.MenuItemId,
                                                    x.MenuItemName,
                                                    x.CalculatePrice,
                                                    x.DecreaseInventory,
                                                    x.Price,
                                                    x.TaxAmount,
                                                    x.TaxTemplateId,
                                                    x.TaxIncluded,
                                                    x.PortionName,
                                                    x.PortionCount,
                                                    x.OrderState,
                                                    x.OrderStateGroupName,
                                                    x.OrderStateGroupId
                                                });

            var result = group.Select(x => new Order
                                    {
                                        MenuItemId = x.Key.MenuItemId,
                                        MenuItemName = x.Key.MenuItemName,
                                        CalculatePrice = x.Key.CalculatePrice,
                                        DecreaseInventory = x.Key.DecreaseInventory,
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
                                        OrderState = x.Key.OrderState,
                                        OrderStateGroupName = x.Key.OrderStateGroupName,
                                        OrderStateGroupId = x.Key.OrderStateGroupId,
                                        Quantity = x.Sum(y => y.Quantity)
                                    });

            result = result.Union(lines.Where(x => x.OrderTagValues.Count > 0)).OrderBy(x => x.CreatedDateTime);

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

    }
}
