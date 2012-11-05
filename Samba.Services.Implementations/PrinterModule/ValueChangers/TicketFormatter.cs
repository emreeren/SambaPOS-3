using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TicketFormatter
    {
        private readonly IAutomationService _automationService;
        private readonly ISettingService _settingService;
        private readonly TicketValueChanger _ticketValueChanger = new TicketValueChanger();

        public TicketFormatter(IAutomationService automationService, ISettingService settingService)
        {
            _automationService = automationService;
            _settingService = settingService;
        }

        public string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate printerTemplate)
        {
            var orders = printerTemplate.MergeLines ? MergeLines(lines.ToList()) : lines;
            ticket.Orders.Clear();
            orders.ToList().ForEach(ticket.Orders.Add);
            var content = _ticketValueChanger.GetValue(printerTemplate, ticket);
            content = UpdateExpressions(content);
            return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private string UpdateExpressions(string data)
        {
            data = _automationService.ReplaceExpressionValues(data);
            data = _settingService.ReplaceSettingValues(data);
            return data;
        }

        private static IEnumerable<Order> MergeLines(IList<Order> orders)
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
