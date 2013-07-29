using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Helpers;
using Samba.Services.Implementations.PrinterModule.ValueChangers;

namespace Samba.Services.Implementations.PrinterModule
{
    [Export]
    public class TicketFormatter
    {
        private readonly IExpressionService _expressionService;
        private readonly ISettingService _settingService;
        private readonly TicketValueChanger _ticketValueChanger;

        [ImportingConstructor]
        public TicketFormatter(IExpressionService expressionService, ISettingService settingService, TicketValueChanger ticketValueChanger)
        {
            _expressionService = expressionService;
            _settingService = settingService;
            _ticketValueChanger = ticketValueChanger;
        }

        public string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate printerTemplate)
        {
            var dataObject = new { Ticket = ObjectCloner.Clone2(ticket) };
            var orders = printerTemplate.MergeLines ? MergeLines(lines.ToList()) : lines.ToList();
            ticket.Orders.Clear();
            orders.ToList().ForEach(ticket.Orders.Add);
            var content = _ticketValueChanger.GetValue(printerTemplate, ticket);
            content = UpdateExpressions(content, dataObject);
            return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private string UpdateExpressions(string data, object dataObject)
        {
            data = _expressionService.ReplaceExpressionValues(data, dataObject.ToDynamic());
            data = _settingService.ReplaceSettingValues(data);
            return data;
        }

        private static IEnumerable<Order> MergeLines(IList<Order> orders)
        {
            var group = orders.Where(x => x.GetOrderTagValues().Count(y => y.Price != 0) == 0).GroupBy(x => new
                                                {
                                                    x.MenuItemId,
                                                    x.MenuItemName,
                                                    x.CalculatePrice,
                                                    x.DecreaseInventory,
                                                    x.IncreaseInventory,
                                                    x.Price,
                                                    x.Taxes,
                                                    x.PortionName,
                                                    x.PortionCount,
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
                                        Taxes = x.Key.Taxes,
                                        CreatedDateTime = x.Last().CreatedDateTime,
                                        CreatingUserName = x.Last().CreatingUserName,
                                        OrderNumber = x.Last().OrderNumber,
                                        TicketId = x.Last().TicketId,
                                        PortionName = x.Key.PortionName,
                                        PortionCount = x.Key.PortionCount,
                                        OrderTags = JsonHelper.Serialize(x.SelectMany(y => y.GetOrderTagValues()).Distinct().ToList()),
                                        OrderStates = JsonHelper.Serialize(x.SelectMany(y => y.GetOrderStateValues()).Distinct().ToList()),
                                        Quantity = x.Sum(y => y.Quantity)
                                    });

            result = result.Union(orders.Where(x => x.GetOrderTagValues().Count(y => y.Price != 0) > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }
    }
}
