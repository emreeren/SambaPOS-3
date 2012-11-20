using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.DaoClasses;
using Samba.Presentation.Services.Common;
using Samba.Presentation.Services.Implementations.PrinterModule.PrintJobs;
using Samba.Presentation.Services.Implementations.PrinterModule.Tools;
using Samba.Presentation.Services.Implementations.PrinterModule.ValueChangers;

namespace Samba.Presentation.Services.Implementations.PrinterModule
{
    [Export(typeof(IPrinterService))]
    public class PrinterService : AbstractService, IPrinterService
    {
        private readonly IPrinterDao _printerDao;
        private readonly IApplicationState _applicationState;
        private readonly TicketFormatter _ticketFormatter;

        [ImportingConstructor]
        public PrinterService(IPrinterDao printerDao, IApplicationState applicationState, IResourceService resourceService,
            IAutomationService automationService, ISettingService settingService)
        {
            _printerDao = printerDao;
            _applicationState = applicationState;
            _ticketFormatter = new TicketFormatter(automationService, settingService);
        }

        private IEnumerable<Printer> _printers;
        public IEnumerable<Printer> Printers
        {
            get { return _printers ?? (_printers = _printerDao.GetPrinters()); }
        }

        private IEnumerable<PrinterTemplate> _printerTemplates;
        protected IEnumerable<PrinterTemplate> PrinterTemplates
        {
            get { return _printerTemplates ?? (_printerTemplates = GetAllPrinterTemplates()); }
        }

        public Printer PrinterById(int id)
        {
            return Printers.Single(x => x.Id == id);
        }

        private PrinterTemplate PrinterTemplateById(int printerTemplateId)
        {
            return PrinterTemplates.Single(x => x.Id == printerTemplateId);
        }

        public IEnumerable<PrinterTemplate> GetAllPrinterTemplates()
        {
            return _printerDao.GetPrinterTemplates();
        }

        public IEnumerable<string> GetPrinterNames()
        {
            return PrinterInfo.GetPrinterNames();
        }

        public IEnumerable<Printer> GetPrinters()
        {
            return Printers;
        }

        private PrinterMap GetPrinterMapForItem(IEnumerable<PrinterMap> printerMaps, int menuItemId)
        {
            var menuItemGroupCode = _printerDao.GetMenuItemGroupCode(menuItemId);
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

        public void PrintTicket(Ticket ticket, PrintJob customPrinter, Func<Order, bool> orderSelector)
        {
            Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
            if (customPrinter.LocksTicket) ticket.RequestLock();
            PrintOrders(customPrinter, ticket, orderSelector);
        }

        public void PrintOrders(PrintJob printJob, Ticket ticket, Func<Order, bool> orderSelector)
        {
            ticket = ObjectCloner.Clone2(ticket);
            if (printJob.ExcludeTax)
            {
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
                case (int)WhatToPrintTypes.LastLinesByPrinterLineCount:
                    ti = GetLastOrders(ticket, printJob);
                    break;
                case (int)WhatToPrintTypes.LastPaidOrders:
                    ti = GetLastPaidOrders(ticket);
                    break;
                default:
                    ti = ticket.Orders.Where(orderSelector).OrderBy(x => x.Id).ToList();
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

        private IEnumerable<Order> GetLastPaidOrders(Ticket ticket)
        {
            IEnumerable<PaidItem> paidItems = _applicationState.LastPaidItems.ToList();
            var result = paidItems.Select(x => ticket.Orders.First(y => y.MenuItemId + "_" + y.Price == x.Key)).ToList();
            foreach (var order in result)
            {
                order.Quantity = paidItems.First(x => x.Key == order.MenuItemId + "_" + order.Price).Quantity;
            }
            return result;
        }

        private IEnumerable<Order> GetLastOrders(Ticket ticket, PrintJob printJob)
        {
            if (ticket.Orders.Count > 1)
            {
                var printMap = printJob.PrinterMaps.Count == 1 ? printJob.PrinterMaps[0]
                    : GetPrinterMapForItem(printJob.PrinterMaps, ticket.Orders.Last().MenuItemId);
                var result = ticket.Orders.OrderByDescending(x => x.CreatedDateTime).ToList();
                var printer = PrinterById(printMap.PrinterId);
                if (printer.PageHeight > 0)
                    result = result.Take(printer.PageHeight).ToList();
                return result;
            }
            return ticket.Orders.ToList();
        }

        private IEnumerable<Order> GroupLinesByValue(Ticket ticket, Expression<Func<MenuItem, string>> selector, string defaultValue, bool calcDiscounts = false)
        {
            var discounts = calcDiscounts ? ticket.GetPreTaxServicesTotal() : 0;
            var di = discounts > 0 ? discounts / ticket.GetPlainSum() : 0;
            var cache = new Dictionary<string, decimal>();
            foreach (var order in ticket.Orders.OrderBy(x => x.Id).ToList())
            {
                var item = order;
                var value = _printerDao.GetMenuItemData(item.MenuItemId, selector);
                //var value = selector(_cacheService.GetMenuItem(x => x.Id == item.MenuItemId)).ToString();
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

        private void InternalPrintOrders(PrintJob printJob, Ticket ticket, IEnumerable<Order> orders)
        {
            if (printJob.PrinterMaps.Count == 1
                && printJob.PrinterMaps[0].MenuItemId == 0
                && printJob.PrinterMaps[0].MenuItemGroupCode == null)
            {
                PrintOrderLines(ticket, orders, printJob.PrinterMaps[0]);
                return;
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

            foreach (var order in ordersCache)
            {
                PrintOrderLines(ticket, order.Value, order.Key);
            }
        }

        private void PrintOrderLines(Ticket ticket, IEnumerable<Order> lines, PrinterMap p)
        {
            Debug.Assert(lines != null, "lines != null");
            var lns = lines.ToList();
            if (!lns.Any()) return;
            if (p == null)
            {
                MessageBox.Show(Resources.GeneralPrintErrorMessage);
                AppServices.Log(Resources.GeneralPrintErrorMessage);
                return;
            }
            var printer = PrinterById(p.PrinterId);
            var prinerTemplate = PrinterTemplateById(p.PrinterTemplateId);
            if (printer == null || string.IsNullOrEmpty(printer.ShareName) || prinerTemplate == null) return;
            var ticketLines = _ticketFormatter.GetFormattedTicket(ticket, lns, prinerTemplate);
            PrintJobFactory.CreatePrintJob(printer).DoPrint(ticketLines);
        }

        public void PrintReport(FlowDocument document,Printer printer)
        {
            if (printer == null || string.IsNullOrEmpty(printer.ShareName)) return;
            PrintJobFactory.CreatePrintJob(printer).DoPrint(document);
        }

        public void ExecutePrintJob(PrintJob printJob)
        {
            if (printJob.PrinterMaps.Count > 0)
            {
                var printerMap = printJob.PrinterMaps[0];
                var printerTemplate = PrinterTemplates.Single(x => x.Id == printerMap.PrinterTemplateId);
                var content = printerTemplate.Template.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                PrintJobFactory.CreatePrintJob(PrinterById(printerMap.PrinterId)).DoPrint(content);
            }
        }

        public IDictionary<string, string> GetTagDescriptions()
        {
            return FunctionRegistry.Descriptions;
        }

        public override void Reset()
        {
            _printers = null;
            _printerTemplates = null;
            PrinterInfo.ResetCache();
        }
    }
}
