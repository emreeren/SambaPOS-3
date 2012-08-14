using System;
using System.Collections.Generic;
using System.Linq;
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
            return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private static IEnumerable<Order> MergeLines(IEnumerable<Order> lines)
        {
            var group = lines.Where(x => x.OrderTagValues.Count(y => y.Price != 0) == 0).GroupBy(x => new
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

            result = result.Union(lines.Where(x => x.OrderTagValues.Count(y => y.Price != 0) > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }

    }
}
