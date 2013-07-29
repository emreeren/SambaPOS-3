using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations.PrinterModule
{
    [Export]
    class TicketPrintTaskBuilder
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
                default:
                    ti = ticket.Orders.Where(orderSelector).OrderBy(x => x.Id).ToList();
                    break;
            }
            return ti;
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

            result.AddRange(ordersCache.Select(order => GetPrintTask(ticket, order.Value, order.Key)));
            return result;
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
            if (printer == null || string.IsNullOrEmpty(printer.ShareName) || prinerTemplate == null) return null;
            if (!printer.IsCustomPrinter && !lns.Any()) return null;
            var ticketLines = _ticketFormatter.GetFormattedTicket(ticket, lns, prinerTemplate);
            return new TicketPrintTask { Lines = ticketLines, Printer = printer };
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
