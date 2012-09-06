using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NCalc;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public static class TicketFormatter
    {
        private static readonly TicketValueChanger TicketValueChanger = new TicketValueChanger();

        public static string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate printerTemplate)
        {
            var orders = printerTemplate.MergeLines ? MergeLines(lines) : lines;
            ticket.Orders.Clear();
            orders.ToList().ForEach(ticket.Orders.Add);
            var content = TicketValueChanger.GetValue(printerTemplate, ticket);
            content = UpdateExpressions(content);
            return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private static string UpdateExpressions(string data)
        {
            while (Regex.IsMatch(data, "\\[=[^\\]]+\\]", RegexOptions.Singleline))
            {
                var match = Regex.Match(data, "\\[=([^\\]]+)\\]");
                var tag = match.Groups[0].Value;
                var expression = match.Groups[1].Value;
                var e = new Expression(expression);
                e.EvaluateFunction += delegate(string name, FunctionArgs args)
                {
                    if (name == "Format" || name == "F")
                    {
                        var fmt = args.Parameters.Length > 1
                                      ? args.Parameters[1].Evaluate().ToString()
                                      : "#,#0.00";
                        args.Result = ((double)args.Parameters[0].Evaluate()).ToString(fmt);
                    }
                    if (name == "ToNumber" || name == "TN")
                    {
                        double d;
                        double.TryParse(args.Parameters[0].Evaluate().ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out d);
                        args.Result = d;
                    }
                };
                string result;
                try
                {
                    result = e.Evaluate().ToString();
                }
                catch (EvaluationException)
                {
                    result = "";
                }

                data = data.Replace(tag, result);
            }

            return data;
        }

        private static IEnumerable<Order> MergeLines(IEnumerable<Order> lines)
        {
            var group = lines.Where(x => x.OrderTagValues.Count(y => y.Price != 0) == 0).GroupBy(x => new
                                                {
                                                    x.MenuItemId,
                                                    x.MenuItemName,
                                                    x.CalculatePrice,
                                                    x.DecreaseInventory,
                                                    x.IncreaseInventory,
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
                                        IncreaseInventory = x.Key.IncreaseInventory,
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

            result = result.Union(lines.Where(x => x.OrderTagValues.Count(y => y.Price != 0) > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }

    }
}
