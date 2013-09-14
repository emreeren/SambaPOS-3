using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    public static class PrintJobFactory
    {
        public static AbstractPrintJob CreatePrintJob(Printer printer, IPrinterService printerService)
        {
            if (printer.IsTextPrinter)
                return new TextPrinterJob(printer);
            if (printer.IsHtmlPrinter)
                return new HtmlPrinterJob(printer);
            if (printer.IsPortPrinter)
                return new PortPrinterJob(printer);
            if (printer.IsDemoPrinter)
                return new DemoPrinterJob(printer);
            if (printer.IsWindowsPrinter)
                return new WindowsPrinterJob(printer);
            if (printer.IsCustomPrinter)
                return new CustomPrinterJob(printer, printerService);          
            if (printer.IsRawPrinter)
                return new RawPrinterJob(printer);
            return new SlipPrinterJob(printer);
        }
    }
}
