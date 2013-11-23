using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations.PrinterModule
{
    [Export]
    public class TicketPrintTaskBuilder
    {
        private readonly ILogService _logService;
        private readonly ICacheService _cacheService;
        private readonly TicketFormatter _ticketFormatter;

        [ImportingConstructor]
        public TicketPrintTaskBuilder(ILogService logService, ICacheService cacheService, TicketFormatter ticketFormatter)
        {
            _logService = logService;
            _cacheService = cacheService;
            _ticketFormatter = ticketFormatter;
        }

        public Printer PrinterById(int id)
        {
            return _cacheService.GetPrinters().Single(x => x.Id == id);
        }

        private PrinterTemplate PrinterTemplateById(int printerTemplateId)
        {
            return _cacheService.GetPrinterTemplates().Single(x => x.Id == printerTemplateId);
        }

        private IEnumerable<Order> GetOrders(PrintJob printJob, Ticket ticket, Func<Order, bool> orderSelector)
        {
            if (printJob.ExcludeTax)
                ticket.TaxIncluded = false;
            IEnumerable<Order> ti;
            switch (printJob.WhatToPrint)
            {
                case (int)WhatToPrintTypes.LastLinesByPrinterLineCount:
                    ti = GetLastOrders(ticket, printJob, orderSelector);
                    break;
                case (int)WhatToPrintTypes.LastPaidOrders:
                    ti = GetLastPaidOrders(ticket);
                    break;
                case (int)WhatToPrintTypes.OrdersByQuanity:
                    ti = SeparateOrders(ticket, orderSelector).OrderBy(x => x.MenuItemName);
                    break;
                case (int)WhatToPrintTypes.SeparatedByQuantity:
                    ti = SeparateOrders(ticket, orderSelector).OrderBy(x => x.MenuItemName);
                    break;
                default:
                    ti = ticket.Orders.Where(orderSelector).OrderBy(x => x.Id).ToList();
                    break;
            }
            return ti;
        }

        private static IEnumerable<Order> SeparateOrders(Ticket ticket, Func<Order, bool> orderSelector)
        {
            var result = new List<Order>();
            foreach (var order in ticket.Orders.Where(orderSelector))
            {
                if (order.Quantity == 1) result.Add(order);
                else
                {
                    for (int i = 0; i < order.Quantity; i++)
                    {
                        var copiedOrder = ObjectCloner.Clone2(order);
                        copiedOrder.Quantity = 1;
                        result.Add(copiedOrder);
                    }
                }
            }
            return result;
        }

        private static IEnumerable<Order> GetLastPaidOrders(Ticket ticket)
        {
            IEnumerable<PaidItem> paidItems = ticket.GetPaidItems().ToList();
            var result = paidItems.Select(x => ticket.Orders.First(y => y.MenuItemId + "_" + y.Price == x.Key)).ToList();
            foreach (var order in result)
            {
                order.Quantity = paidItems.First(x => x.Key == order.MenuItemId + "_" + order.Price).Quantity;
            }
            return result;
        }

        private IEnumerable<Order> GetLastOrders(Ticket ticket, PrintJob printJob, Func<Order, bool> orderSelector)
        {
            if (ticket.Orders.Count > 1)
            {
                var printMap = printJob.PrinterMaps.Count == 1 ? printJob.PrinterMaps[0]
                    : GetPrinterMapForItem(printJob.PrinterMaps, ticket.Orders.Last().MenuItemId);
                var result = ticket.Orders.Where(orderSelector).OrderByDescending(x => x.CreatedDateTime).ToList();
                var printer = PrinterById(printMap.PrinterId);
                if (printer.PageHeight > 0)
                    result = result.Take(printer.PageHeight).ToList();
                return result;
            }
            return ticket.Orders.ToList();
        }

        private IEnumerable<TicketPrintTask> GetPrintTasks(PrintJob printJob, Ticket ticket, IEnumerable<Order> orders)
        {
            var result = new List<TicketPrintTask>();

            if (printJob.PrinterMaps.Count == 1
                && printJob.WhatToPrint != 3
                && printJob.WhatToPrint != 4
                && printJob.PrinterMaps[0].MenuItemId == 0
                && printJob.PrinterMaps[0].MenuItemGroupCode == null)
            {
                result.Add(GetPrintTask(ticket, orders, printJob.PrinterMaps[0]));
                return result;
            }

            var ordersCache = new Dictionary<PrinterMap, IList<Order>>();

            foreach (var item in orders)
            {
                var p = GetPrinterMapForItem(printJob.PrinterMaps, item.MenuItemId);
                if (p != null)
                {
                    var lmap = p;
                    var pmap = ordersCache.SingleOrDefault(
                            x => x.Key.PrinterId == lmap.PrinterId && x.Key.PrinterTemplateId == lmap.PrinterTemplateId).Key;
                    if (pmap == null)
                        ordersCache.Add(p, new List<Order>());
                    else p = pmap;
                    ordersCache[p].Add(item);
                }
            }
            switch (printJob.WhatToPrintType)
            {
                case WhatToPrintTypes.SeparatedByQuantity: result.AddRange(GenerateSeparatedTasks(ticket, ordersCache));
                    break;
                default:
                    result.AddRange(ordersCache.Select(order => GetPrintTask(ticket, order.Value, order.Key)));
                    break;
            }
            return result;
        }

        private IEnumerable<TicketPrintTask> GenerateSeparatedTasks(Ticket ticket, Dictionary<PrinterMap, IList<Order>> ordersCache)
        {
            return (from item in ordersCache from order in item.Value select GetPrintTask(ticket, new[] { order }, item.Key)).ToList();
        }

        private TicketPrintTask GetPrintTask(Ticket ticket, IEnumerable<Order> orders, PrinterMap map)
        {
            Debug.Assert(orders != null, "orders != null");
            var lns = orders.ToList();
            if (map == null)
            {
                MessageBox.Show(Resources.GeneralPrintErrorMessage);
                _logService.Log(Resources.GeneralPrintErrorMessage);
                return null;
            }
            var printer = PrinterById(map.PrinterId);
            var prinerTemplate = PrinterTemplateById(map.PrinterTemplateId);
            if (ShouldSkipPrint(printer, lns, prinerTemplate)) return null;
            var ticketLines = _ticketFormatter.GetFormattedTicket(ticket, lns, prinerTemplate);
            return new TicketPrintTask { Lines = ticketLines, Printer = printer };
        }

        private static bool ShouldSkipPrint(Printer printer, IEnumerable<Order> lns, PrinterTemplate prinerTemplate)
        {
            if (printer == null || string.IsNullOrEmpty(printer.ShareName) || prinerTemplate == null) return true;
            if (printer.IsCustomPrinter) return false;
            return (!lns.Any() && prinerTemplate.Template.Contains("{ORDERS}"));
        }

        private PrinterMap GetPrinterMapForItem(IEnumerable<PrinterMap> printerMaps, int menuItemId)
        {
            var menuItemGroupCode = _cacheService.GetMenuItemData(menuItemId, x => x.GroupCode);
            Debug.Assert(printerMaps != null);
            var maps = printerMaps.ToList();

            maps = maps.Count(x => x.MenuItemGroupCode == menuItemGroupCode) > 0
                       ? maps.Where(x => x.MenuItemGroupCode == menuItemGroupCode).ToList()
                       : maps.Where(x => x.MenuItemGroupCode == null).ToList();

            maps = maps.Count(x => x.MenuItemId == menuItemId) > 0
                       ? maps.Where(x => x.MenuItemId == menuItemId).ToList()
                       : maps.Where(x => x.MenuItemId == 0).ToList();

            return maps.FirstOrDefault();
        }

        public IEnumerable<TicketPrintTask> GetPrintTasksForTicket(Ticket ticket, PrintJob printJob,
                                                                   Func<Order, bool> orderSelector)
        {
            var orders = GetOrders(printJob, ticket, orderSelector);
            return GetPrintTasks(printJob, ticket, orders);
        }
    }
}
