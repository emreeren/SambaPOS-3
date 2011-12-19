using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Linq;
using System.Windows.Threading;
using Samba.Domain;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.PrinterModule.ServiceImplementations
{
    internal class PrinterData
    {
        public Printer Printer { get; set; }
        public PrinterTemplate PrinterTemplate { get; set; }
        public Ticket Ticket { get; set; }
    }

    internal class TicketData
    {
        public Ticket Ticket { get; set; }
        public IEnumerable<Order> Orders { get; set; }
        public PrintJob PrintJob { get; set; }
    }

    [Export(typeof(IPrinterService))]
    public class PrinterService : AbstractService, IPrinterService
    {
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public PrinterService(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public IEnumerable<string> GetPrinterNames()
        {
            return PrinterInfo.GetPrinterNames();
        }

        public override void Reset()
        {
            PrinterInfo.ResetCache();
        }

        private static PrinterMap GetPrinterMapForItem(IEnumerable<PrinterMap> printerMaps, Ticket ticket, int menuItemId)
        {
            var menuItemGroupCode = Dao.Single<MenuItem, string>(menuItemId, x => x.GroupCode);

            var maps = printerMaps;

            maps = maps.Count(x => !string.IsNullOrEmpty(x.TicketTag) && !string.IsNullOrEmpty(ticket.GetTagValue(x.TicketTag))) > 0
                ? maps.Where(x => !string.IsNullOrEmpty(x.TicketTag) && !string.IsNullOrEmpty(ticket.GetTagValue(x.TicketTag)))
                : maps.Where(x => string.IsNullOrEmpty(x.TicketTag));

            maps = maps.Count(x => x.Department != null && x.Department.Id == ticket.DepartmentId) > 0
                       ? maps.Where(x => x.Department != null && x.Department.Id == ticket.DepartmentId)
                       : maps.Where(x => x.Department == null);

            maps = maps.Count(x => x.MenuItemGroupCode == menuItemGroupCode) > 0
                       ? maps.Where(x => x.MenuItemGroupCode == menuItemGroupCode)
                       : maps.Where(x => x.MenuItemGroupCode == null);

            maps = maps.Count(x => x.MenuItem != null && x.MenuItem.Id == menuItemId) > 0
                       ? maps.Where(x => x.MenuItem != null && x.MenuItem.Id == menuItemId)
                       : maps.Where(x => x.MenuItem == null);

            return maps.FirstOrDefault();
        }

        public void AutoPrintTicket(Ticket ticket)
        {
            foreach (var customPrinter in AppServices.CurrentTerminal.PrintJobs.Where(x => !x.UseForPaidTickets))
            {
                if (ShouldAutoPrint(ticket, customPrinter))
                    ManualPrintTicket(ticket, customPrinter);
            }
        }

        public void ManualPrintTicket(Ticket ticket, PrintJob customPrinter)
        {
            Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
            if (customPrinter.LocksTicket) ticket.RequestLock();
            ticket.AddPrintJob(customPrinter.Id);
            PrintOrders(customPrinter, ticket);
        }

        private static bool ShouldAutoPrint(Ticket ticket, PrintJob customPrinter)
        {
            if (customPrinter.WhenToPrint == (int)WhenToPrintTypes.Manual) return false;
            if (customPrinter.WhenToPrint == (int)WhenToPrintTypes.Paid)
            {
                if (ticket.DidPrintJobExecuted(customPrinter.Id)) return false;
                if (!ticket.IsPaid) return false;
                if (!customPrinter.AutoPrintIfCash && !customPrinter.AutoPrintIfCreditCard && !customPrinter.AutoPrintIfTicket) return false;
                if (customPrinter.AutoPrintIfCash && ticket.Payments.Count(x => x.PaymentType == (int)PaymentType.Cash) > 0) return true;
                if (customPrinter.AutoPrintIfCreditCard && ticket.Payments.Count(x => x.PaymentType == (int)PaymentType.CreditCard) > 0) return true;
                if (customPrinter.AutoPrintIfTicket && ticket.Payments.Count(x => x.PaymentType == (int)PaymentType.Ticket) > 0) return true;
            }
            if (customPrinter.WhenToPrint == (int)WhenToPrintTypes.NewLinesAdded && ticket.GetUnlockedOrders().Count() > 0) return true;
            return false;
        }

        public void PrintOrders(PrintJob printJob, Ticket ticket)
        {
            if (printJob.ExcludeTax)
            {
                ticket = ObjectCloner.Clone(ticket);
                ticket.Orders.ToList().ForEach(x => x.TaxIncluded = false);
            }

            IEnumerable<Order> ti;
            switch (printJob.WhatToPrint)
            {
                case (int)WhatToPrintTypes.NewLines:
                    ti = ticket.GetUnlockedOrders();
                    break;
                case (int)WhatToPrintTypes.GroupedByBarcode:
                    ti = GroupLinesByValue(ticket, x => x.Barcode ?? "", "1", true);
                    break;
                case (int)WhatToPrintTypes.GroupedByGroupCode:
                    ti = GroupLinesByValue(ticket, x => x.GroupCode ?? "", Resources.UndefinedWithBrackets);
                    break;
                case (int)WhatToPrintTypes.GroupedByTag:
                    ti = GroupLinesByValue(ticket, x => x.Tag ?? "", Resources.UndefinedWithBrackets);
                    break;
                default:
                    ti = ticket.Orders.OrderBy(x => x.Id).ToList();
                    break;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new Action(
                    delegate
                    {
                        try
                        {
                            InternalPrintOrders(printJob, ticket, ti);
                        }
                        catch (Exception e)
                        {
                            AppServices.LogError(e, "Yazdırma işlemi sırasında bir sorun meydana geldi. Lütfen yazıcı ve şablon ayarlarını kontrol ediniz.\r\n\r\nMesaj:\r\n" + e.Message);
                        }
                    }));
        }

        private IEnumerable<Order> GroupLinesByValue(Ticket ticket, Func<MenuItem, object> selector, string defaultValue, bool calcDiscounts = false)
        {
            var discounts = calcDiscounts ? ticket.GetDiscountAndRoundingTotal() : 0;
            var di = discounts > 0 ? discounts / ticket.GetPlainSum() : 0;
            var cache = new Dictionary<string, decimal>();
            foreach (var order in ticket.Orders.OrderBy(x => x.Id).ToList())
            {
                var item = order;
                var value = selector(_menuService.GetMenuItem(item.MenuItemId)).ToString();
                if (string.IsNullOrEmpty(value)) value = defaultValue;
                if (!cache.ContainsKey(value))
                    cache.Add(value, 0);
                var total = (item.GetTotal());
                cache[value] += Decimal.Round(total - (total * di), 2);
            }
            return cache.Select(x => new Order
            {
                MenuItemName = x.Key,
                Price = x.Value,
                Quantity = 1,
                PortionCount = 1
            });
        }

        private static void InternalPrintOrders(PrintJob printJob, Ticket ticket, IEnumerable<Order> orders)
        {
            if (printJob.PrinterMaps.Count == 1
                && printJob.PrinterMaps[0].TicketTag == null
                && printJob.PrinterMaps[0].MenuItem == null
                && printJob.PrinterMaps[0].MenuItemGroupCode == null
                && printJob.PrinterMaps[0].Department == null)
            {
                PrintOrderLines(ticket, orders, printJob.PrinterMaps[0]);
                return;
            }

            var ordersCache = new Dictionary<PrinterMap, IList<Order>>();

            foreach (var item in orders)
            {
                var p = GetPrinterMapForItem(printJob.PrinterMaps, ticket, item.MenuItemId);
                if (p != null)
                {
                    var lmap = p;
                    var pmap = ordersCache.SingleOrDefault(
                            x => x.Key.Printer == lmap.Printer && x.Key.PrinterTemplate == lmap.PrinterTemplate).Key;
                    if (pmap == null)
                        ordersCache.Add(p, new List<Order>());
                    else p = pmap;
                    ordersCache[p].Add(item);
                }
            }

            foreach (var order in ordersCache)
            {
                PrintOrderLines(ticket, order.Value, order.Key);
            }
        }

        private static void PrintOrderLines(Ticket ticket, IEnumerable<Order> lines, PrinterMap p)
        {
            if (lines.Count() <= 0) return;
            if (p == null)
            {
                MessageBox.Show("Yazdırma sırasında bir problem tespit edildi: Yazıcı Haritası null");
                AppServices.Log("Yazıcı Haritası NULL problemi tespit edildi.");
                return;
            }
            if (p.Printer == null || string.IsNullOrEmpty(p.Printer.ShareName) || p.PrinterTemplate == null) return;
            var ticketLines = TicketFormatter.GetFormattedTicket(ticket, lines, p.PrinterTemplate);
            PrintJobFactory.CreatePrintJob(p.Printer).DoPrint(ticketLines);
        }

        public void PrintReport(FlowDocument document)
        {
            var printer = AppServices.CurrentTerminal.ReportPrinter;
            if (printer == null || string.IsNullOrEmpty(printer.ShareName)) return;
            PrintJobFactory.CreatePrintJob(printer).DoPrint(document);
        }

        public void PrintSlipReport(FlowDocument document)
        {
            var printer = AppServices.CurrentTerminal.SlipReportPrinter;
            if (printer == null || string.IsNullOrEmpty(printer.ShareName)) return;
            PrintJobFactory.CreatePrintJob(printer).DoPrint(document);
        }
    }
}
