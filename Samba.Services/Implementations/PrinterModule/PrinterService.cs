using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Services.Common;
using Samba.Services.Implementations.PrinterModule.Formatters;
using Samba.Services.Implementations.PrinterModule.PrintJobs;
using Samba.Services.Implementations.PrinterModule.Tools;
using Samba.Services.Implementations.PrinterModule.ValueChangers;

namespace Samba.Services.Implementations.PrinterModule
{
    [Export(typeof(IPrinterService))]
    public class PrinterService : IPrinterService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogService _logService;
        private readonly TicketFormatter _ticketFormatter;
        private readonly EntityFormatter _entityFormatter;
        private readonly AccountTransactionDocumentFormatter _accountTransactionDocumentFormatter;
        private readonly FunctionRegistry _functionRegistry;

        [ImportingConstructor]
        public PrinterService(ISettingService settingService, ICacheService cacheService, IExpressionService expressionService, ILogService logService,
            TicketFormatter ticketFormatter, EntityFormatter entityFormatter, AccountTransactionDocumentFormatter accountTransactionDocumentFormatter, FunctionRegistry functionRegistry)
        {
            _cacheService = cacheService;
            _logService = logService;
            _ticketFormatter = ticketFormatter;
            _entityFormatter = entityFormatter;
            _accountTransactionDocumentFormatter = accountTransactionDocumentFormatter;
            _functionRegistry = functionRegistry;
            _functionRegistry.RegisterFunctions();
        }

        [ImportMany]
        public IEnumerable<IPrinterProcessor> PrinterProcessors { get; set; }

        public IEnumerable<Printer> Printers
        {
            get { return _cacheService.GetPrinters(); }
        }

        protected IEnumerable<PrinterTemplate> PrinterTemplates
        {
            get { return _cacheService.GetPrinterTemplates(); }
        }

        public Printer PrinterById(int id)
        {
            return Printers.Single(x => x.Id == id);
        }

        private PrinterTemplate PrinterTemplateById(int printerTemplateId)
        {
            return PrinterTemplates.Single(x => x.Id == printerTemplateId);
        }

        public IEnumerable<string> GetPrinterNames()
        {
            return PrinterInfo.GetPrinterNames();
        }

        public IEnumerable<string> GetProcessorNames()
        {
            return PrinterProcessors.Select(x => x.Name);
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

        public void PrintTicket(Ticket ticket, PrintJob printJob, Func<Order, bool> orderSelector)
        {
            Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
            PrintOrders(printJob, ticket, orderSelector);
        }

        public void PrintAccountTransactionDocument(AccountTransactionDocument document, Printer printer, PrinterTemplate printerTemplate)
        {
            var lines = _accountTransactionDocumentFormatter.GetFormattedDocument(document, printerTemplate);
            if (lines != null)
            {
                PrintJobFactory.CreatePrintJob(printer).DoPrint(lines);
            }
        }

        public void PrintEntity(Entity entity, Printer printer, PrinterTemplate printerTemplate)
        {
            var lines = _entityFormatter.GetFormattedDocument(entity, printerTemplate);
            if (lines != null)
            {
                PrintJobFactory.CreatePrintJob(printer).DoPrint(lines);
            }
        }

        public void PrintOrders(PrintJob printJob, Ticket ticket, Func<Order, bool> orderSelector)
        {
            ticket = ObjectCloner.Clone2(ticket);
            if (printJob.ExcludeTax)
                ticket.TaxIncluded = false;
            IEnumerable<Order> ti;
            switch (printJob.WhatToPrint)
            {
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
                            _logService.LogError(e, Resources.PrintErrorMessage + e.Message);
                        }
                    }));
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

        private void PrintOrderLines(Ticket ticket, IEnumerable<Order> orders, PrinterMap map)
        {
            Debug.Assert(orders != null, "orders != null");
            var lns = orders.ToList();
            if (!lns.Any()) return;
            if (map == null)
            {
                MessageBox.Show(Resources.GeneralPrintErrorMessage);
                _logService.Log(Resources.GeneralPrintErrorMessage);
                return;
            }
            var printer = PrinterById(map.PrinterId);
            var prinerTemplate = PrinterTemplateById(map.PrinterTemplateId);
            if (printer == null || string.IsNullOrEmpty(printer.ShareName) || prinerTemplate == null) return;
            var ticketLines = _ticketFormatter.GetFormattedTicket(ticket, lns, prinerTemplate);

            var processor = GetPrinterProcessor(printer.ShareName);
            if (processor != null)
                ticketLines = processor.Process(ticket, lns, ticketLines);
            if (ticketLines != null)
                PrintJobFactory.CreatePrintJob(printer).DoPrint(ticketLines);
        }

        public void PrintReport(FlowDocument document, Printer printer)
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
                var printer = PrinterById(printerMap.PrinterId);
                var processor = GetPrinterProcessor(printer.ShareName);
                if (processor != null)
                {
                    content = processor.Process(null, null, content);
                }
                if (content != null)
                {
                    PrintJobFactory.CreatePrintJob(PrinterById(printerMap.PrinterId)).DoPrint(content);
                }
            }
        }

        public IPrinterProcessor GetPrinterProcessor(string processorName)
        {
            return PrinterProcessors.FirstOrDefault(x => x.Name == processorName);
        }

        public IDictionary<string, string> GetTagDescriptions()
        {
            return _functionRegistry.Descriptions;
        }

        public void ResetCache()
        {
            PrinterInfo.ResetCache();
        }

        public string GetPrintingContent(Ticket ticket, string format, int width)
        {
            var lines = _ticketFormatter.GetFormattedTicket(ticket, ticket.Orders, new PrinterTemplate() { Template = format });
            var result = new FormattedDocument(lines, width).GetFormattedText();
            return result;
        }
    }
}
