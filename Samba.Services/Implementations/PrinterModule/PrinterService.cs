using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Settings;
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
        private readonly FunctionRegistry _functionRegistry;
        private readonly TicketPrintTaskBuilder _ticketPrintTaskBuilder;

        [ImportingConstructor]
        PrinterService(ISettingService settingService, ICacheService cacheService, IExpressionService expressionService, ILogService logService,
            TicketFormatter ticketFormatter, FunctionRegistry functionRegistry, TicketPrintTaskBuilder ticketPrintTaskBuilder)
        {
            _cacheService = cacheService;
            _logService = logService;
            _ticketFormatter = ticketFormatter;
            _functionRegistry = functionRegistry;
            _ticketPrintTaskBuilder = ticketPrintTaskBuilder;
            _functionRegistry.RegisterFunctions();
        }

        [ImportMany]
        public IEnumerable<IDocumentFormatter> DocumentFormatters { get; set; }

        [ImportMany]
        public IEnumerable<ICustomPrinter> CustomPrinters { get; set; }

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

        public IEnumerable<string> GetPrinterNames()
        {
            return PrinterInfo.GetPrinterNames();
        }

        public IEnumerable<string> GetCustomPrinterNames()
        {
            return CustomPrinters.Select(x => x.Name);
        }

        public ICustomPrinter GetCustomPrinter(string customPrinterName)
        {
            return CustomPrinters.FirstOrDefault(x => x.Name == customPrinterName);
        }

        public void PrintTicket(Ticket ticket, PrintJob printJob, Func<Order, bool> orderSelector)
        {
            ticket = ObjectCloner.Clone2(ticket);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(
                        delegate
                        {
                            try
                            {
                                LocalSettings.UpdateThreadLanguage();
                                var tasks = _ticketPrintTaskBuilder.GetPrintTasksForTicket(ticket, printJob, orderSelector);
                                foreach (var ticketPrintTask in tasks.Where(x => x != null && x.Printer != null && x.Lines != null))
                                {
                                    Print(ticketPrintTask.Printer, ticketPrintTask.Lines);
                                }
                            }
                            catch (Exception e)
                            {
                                _logService.LogError(e, Resources.PrintErrorMessage + e.Message);
                            }
                        }));
        }

        public void PrintObject(object item, Printer printer, PrinterTemplate printerTemplate)
        {
            var formatter = DocumentFormatters.FirstOrDefault(x => x.ObjectType == item.GetType());
            if (formatter != null)
            {
                var lines = formatter.GetFormattedDocument(item, printerTemplate);
                if (lines != null)
                {
                    Print(printer, lines);
                }
            }
        }

        public void PrintReport(FlowDocument document, Printer printer)
        {
            if (printer == null || string.IsNullOrEmpty(printer.ShareName)) return;
            Print(printer, document);
        }

        public void ExecutePrintJob(PrintJob printJob)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new Action(
                    delegate
                    {
                        try
                        {
                            LocalSettings.UpdateThreadLanguage();
                            InternalExecutePrintJob(printJob);
                        }
                        catch (Exception e)
                        {
                            _logService.LogError(e, Resources.PrintErrorMessage + e.Message);
                        }
                    }));
        }

        public void InternalExecutePrintJob(PrintJob printJob)
        {
            if (printJob.PrinterMaps.Count > 0)
            {
                var printerMap = printJob.PrinterMaps[0];
                var printerTemplate = PrinterTemplates.Single(x => x.Id == printerMap.PrinterTemplateId);
                var content = printerTemplate.Template.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var printer = PrinterById(printerMap.PrinterId);
                Print(printer, content);
            }
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
            var lines = _ticketFormatter.GetFormattedTicket(ticket, ticket.Orders, new PrinterTemplate { Template = format });
            var result = new FormattedDocument(lines, width).GetFormattedText();
            return result;
        }

        public object GetCustomPrinterData(string customPrinterName, string customPrinterData)
        {
            var printer = GetCustomPrinter(customPrinterName);
            return printer != null ? printer.GetSettingsObject(customPrinterData) : "";
        }

        private void Print(Printer printer, FlowDocument document)
        {
            PrintJobFactory.CreatePrintJob(printer, this).DoPrint(document);
        }

        private void Print(Printer printer, string[] document)
        {
            PrintJobFactory.CreatePrintJob(printer, this).DoPrint(document);
        }
    }
}
