using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public static class TicketFormatter
    {
        private static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        private static readonly TicketValueChanger TicketValueChanger = new TicketValueChanger();
        private static readonly OrderValueChanger OrderValueChanger = new OrderValueChanger();
        private static readonly ResourceValueChanger ResourceValueChanger = new ResourceValueChanger();

        public static string[] GetFormattedTicket(Ticket ticket, IEnumerable<Order> lines, PrinterTemplate template)
        {
            var orders = lines.ToList();

            if (template.MergeLines) orders = MergeLines(orders).ToList();

            string content = TicketValueChanger.GetValue(template, ticket);
            content = ResourceValueChanger.Replace(template, content, ticket.TicketResources);
            content = OrderValueChanger.Replace(template, content, orders);

            content = SettingService.GetSettingReplacer().ReplaceSettingValue("{SETTING:([^}]+)}", content);

            return content.Split(new[] { '\r', '\n' }).ToArray();
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

    }
}
