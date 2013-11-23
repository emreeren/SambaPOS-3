using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.PrintJobs;

namespace Samba.Services.Implementations.PrinterModule
{
    public class ReportPrinter
    {
        private readonly FlowDocument _document;
        private Printer _printer;
        private PrinterService _printerService;
        private ILogService _logService;

        private ReportPrinter(FlowDocument document)
        {
            _document = document;
        }

        public static ReportPrinter For(FlowDocument document)
        {
            return new ReportPrinter(document);
        }

        public ReportPrinter WithPrinter(Printer printer)
        {
            _printer = printer;
            return this;
        }
        public ReportPrinter WithPrinterService(PrinterService printerService)
        {
            _printerService = printerService;
            return this;
        }

        public ReportPrinter WithLogService(ILogService logService)
        {
            _logService = logService;
            return this;
        }

        public void Print()
        {
            if (_printer == null || string.IsNullOrEmpty(_printer.ShareName)) return;
            AsyncPrintTask.Exec(false, () => PrintJobFactory.CreatePrintJob(_printer, _printerService).DoPrint(_document), _logService);
        }
    }
}