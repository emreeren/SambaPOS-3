﻿using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Helpers;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TicketFormatter
    {
        private readonly IExpressionService _expressionService;
        private readonly ISettingService _settingService;

        public TicketFormatter(IExpressionService expressionService, ISettingService settingService)
        {
            _expressionService = expressionService;
            _settingService = settingService;
        }

        private TicketValueChanger _ticketValueChanger;
        private TicketValueChanger TicketValueChanger
        {
            get
            {
                return _ticketValueChanger ??
                    (_ticketValueChanger = new TicketValueChanger());
            }
        }

        public string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate printerTemplate)
        {
            var orders = printerTemplate.MergeLines ? MergeLines(lines.ToList()) : lines;
            ticket.Orders.Clear();
            orders.ToList().ForEach(ticket.Orders.Add);
            var content = TicketValueChanger.GetValue(printerTemplate, ticket);
            content = UpdateExpressions(content);
            return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private string UpdateExpressions(string data)
        {
            data = _expressionService.ReplaceExpressionValues(data);
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
