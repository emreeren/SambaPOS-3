using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;
using Samba.Services.Implementations.PrinterModule.PrintJobs;
using Samba.Services.Implementations.PrinterModule.Tools;
using Samba.Services.Implementations.PrinterModule.ValueChangers;

namespace Samba.Services.Implementations.PrinterModule
{
    [Export(typeof(IPrinterService))]
    public class PrinterService : AbstractService, IPrinterService
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PrinterService(IApplicationState applicationState, ICacheService cacheService, IResourceService resourceService)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            ValidatorRegistry.RegisterDeleteValidator(new PrinterDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator<PrinterTemplate>(x => Dao.Exists<PrinterMap>(y => y.PrinterTemplateId == x.Id), Resources.PrinterTemplate, Resources.PrintJob);
        }

        private IEnumerable<Printer> _printers;
        public IEnumerable<Printer> Printers
        {
            get { return _printers ?? (_printers = Dao.Query<Printer>()); }
        }

        private IEnumerable<PrinterTemplate> _printerTemplates;
        protected IEnumerable<PrinterTemplate> PrinterTemplates
        {
            get { return _printerTemplates ?? (_printerTemplates = Dao.Query<PrinterTemplate>()); }
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
            return Dao.Query<PrinterTemplate>();
        }

        public IEnumerable<string> GetPrinterNames()
        {
            return PrinterInfo.GetPrinterNames();
        }

        public IEnumerable<Printer> GetPrinters()
        {
            return Printers;
        }

        public override void Reset()
        {
            _printers = null;
            _printerTemplates = null;
            PrinterInfo.ResetCache();
        }

        private static PrinterMap GetPrinterMapForItem(IEnumerable<PrinterMap> printerMaps, int menuItemId)
        {
            var menuItemGroupCode = Dao.Single<MenuItem, string>(menuItemId, x => x.GroupCode);
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

        public void PrintTicket(Ticket ticket, PrintJob customPrinter)
        {
            Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
            if (customPrinter.LocksTicket) ticket.RequestLock();
            PrintOrders(customPrinter, ticket);
        }

        public void PrintOrders(PrintJob printJob, Ticket ticket)
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
                    ti = ticket.Orders.OrderBy(x => x.Id).Take(1); // todo: make it configurable
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
            var discounts = calcDiscounts ? ticket.GetPreTaxServicesTotal() : 0;
            var di = discounts > 0 ? discounts / ticket.GetPlainSum() : 0;
            var cache = new Dictionary<string, decimal>();
            foreach (var order in ticket.Orders.OrderBy(x => x.Id).ToList())
            {
                var item = order;
                var value = selector(_cacheService.GetMenuItem(x => x.Id == item.MenuItemId)).ToString();
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
            if (lns.Count() == 0) return;
            if (p == null)
            {
                //todo: globalize
                MessageBox.Show("Yazdırma sırasında bir problem tespit edildi: Yazıcı Haritası null");
                AppServices.Log("Yazıcı Haritası NULL problemi tespit edildi.");
                return;
            }
            var printer = PrinterById(p.PrinterId);
            var prinerTemplate = PrinterTemplateById(p.PrinterTemplateId);
            if (printer == null || string.IsNullOrEmpty(printer.ShareName) || prinerTemplate == null) return;
            var ticketLines = TicketFormatter.GetFormattedTicket(ticket, lns, prinerTemplate);
            PrintJobFactory.CreatePrintJob(printer).DoPrint(ticketLines);
        }

        public void PrintReport(FlowDocument document)
        {
            var printer = _applicationState.CurrentTerminal.ReportPrinter;
            if (printer == null || string.IsNullOrEmpty(printer.ShareName)) return;
            PrintJobFactory.CreatePrintJob(printer).DoPrint(document);
        }

        public void PrintSlipReport(FlowDocument document)
        {
            var printer = _applicationState.CurrentTerminal.SlipReportPrinter;
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
    }

    public class PrinterDeleteValidator : SpecificationValidator<Printer>
    {
        public override string GetErrorMessage(Printer model)
        {
            if (Dao.Exists<Terminal>(x => x.ReportPrinter.Id == model.Id || x.SlipReportPrinter.Id == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Printer, Resources.Terminal);
            if (Dao.Exists<PrinterMap>(x => x.PrinterId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Printer, Resources.PrintJob);
            return "";
        }
    }
}
