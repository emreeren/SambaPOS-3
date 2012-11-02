using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ComLib.Lang;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public static class TicketFormatter
    {
        private static readonly TicketValueChanger TicketValueChanger = new TicketValueChanger();

        private static Interpreter _interpreter;
        internal static Interpreter Interpreter { get { return _interpreter ?? (_interpreter = CreateInterpreter()); } }

        private static Interpreter CreateInterpreter()
        {
            var result = new Interpreter();
            result.Context.Plugins.RegisterAllSystem();
            result.SetFunctionCallback("F", FormatFunction);
            result.SetFunctionCallback("TN", ToNumberFunction);
            return result;
        }

        private static object ToNumberFunction(FunctionCallExpr arg)
        {
            double d;
            double.TryParse(arg.ParamList[0].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out d);
            return d;
        }

        private static object FormatFunction(FunctionCallExpr arg)
        {
            var fmt = arg.ParamList.Count > 1
                          ? arg.ParamList[1].ToString()
                          : "#,#0.00";
            return ((double)arg.ParamList[0]).ToString(fmt);
        }

        internal static string Eval(string expression)
        {
            try
            {
                Interpreter.Execute("result = " + expression);
                return Interpreter.Memory.Get<string>("result");
            }
            catch (Exception)
            {
                return "";
            }
        }

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
                var result = Eval(expression);
                data = data.Replace(tag, result);
            }

            return data;
        }

        private static IEnumerable<Order> MergeLines(IEnumerable<Order> orders)
        {
            var group = orders.Where(x => x.OrderTagValues.Count(y => y.Price != 0) == 0).GroupBy(x => new
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
                                                    x.OrderStateGroupId,
                                                    x.OrderKey
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
                                        OrderTagValues = x.SelectMany(y => y.OrderTagValues).Distinct(OrderTagValueComparer).ToList(),
                                        Quantity = x.Sum(y => y.Quantity)
                                    });

            result = result.Union(orders.Where(x => x.OrderTagValues.Count(y => y.Price != 0) > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }

        private static IEqualityComparer<OrderTagValue> _orderTagValueComparer;
        static IEqualityComparer<OrderTagValue> OrderTagValueComparer
        {
            get { return _orderTagValueComparer ?? (_orderTagValueComparer = new OrderTagComparer()); }
        }
    }

    internal class OrderTagComparer : IEqualityComparer<OrderTagValue>
    {
        public bool Equals(OrderTagValue x, OrderTagValue y)
        {
            return x.TagName == y.TagName && x.TagValue == y.TagValue;
        }

        public int GetHashCode(OrderTagValue obj)
        {
            return (obj.TagName + "_" + obj.TagValue).GetHashCode();
        }
    }
}
