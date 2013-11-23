using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.PrintJobs;

namespace Samba.Services.Implementations.PrinterModule
{
    public class PrintJobExecutor
    {
        private readonly PrintJob _printJob;
        private bool _highPriority;
        private ILogService _logService;
        private ICacheService _cacheService;
        private PrinterService _printerService;

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

        private PrintJobExecutor(PrintJob printJob)
        {
            _printJob = printJob;
        }

        public static PrintJobExecutor For(PrintJob printJob)
        {
            return new PrintJobExecutor(printJob);
        }

        public PrintJobExecutor WithLogSerivce(ILogService logService)
        {
            _logService = logService;
            return this;
        }

        public PrintJobExecutor WithCacheService(ICacheService cacheService)
        {
            _cacheService = cacheService;
            return this;
        }

        public PrintJobExecutor WithPrinterService(PrinterService printerService)
        {
            _printerService = printerService;
            return this;
        }

        public PrintJobExecutor IsHighPriority(bool highPriority)
        {
            _highPriority = highPriority;
            return this;
        }

        public void Execute()
        {
            AsyncPrintTask.Exec(_highPriority, () => InternalExecutePrintJob(_printJob), _logService);
        }

        public void InternalExecutePrintJob(PrintJob printJob)
        {
            if (printJob.PrinterMaps.Count > 0)
            {
                var printerMap = printJob.PrinterMaps[0];
                var printerTemplate = PrinterTemplates.Single(x => x.Id == printerMap.PrinterTemplateId);
                var content = printerTemplate.Template.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var printer = PrinterById(printerMap.PrinterId);
                PrintJobFactory.CreatePrintJob(printer, _printerService).DoPrint(content);
            }
        }
    }
}