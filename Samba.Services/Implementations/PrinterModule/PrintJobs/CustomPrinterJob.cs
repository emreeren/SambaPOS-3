using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.Formatters;
using Samba.Services.Implementations.PrinterModule.Tools;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    public class CustomPrinterJob : AbstractPrintJob
    {
        private readonly IPrinterService _printerService;

        public CustomPrinterJob(Printer printer, IPrinterService printerService)
            : base(printer)
        {
            _printerService = printerService;
        }

        public override void DoPrint(string[] lines)
        {
            var q = PrinterInfo.GetPrinter(Printer.ShareName);
            var text = new FormattedDocument(lines, Printer.CharsPerLine).GetFormattedText();
            var customPrinter = _printerService.GetCustomPrinter(Printer.CustomPrinterName);
            customPrinter.Process(Printer, text);
        }

        public override void DoPrint(FlowDocument document)
        {
            throw new System.NotImplementedException();
        }
    }
}