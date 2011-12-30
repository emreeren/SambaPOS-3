using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    public static class PrintJobFactory
    {
        public static AbstractPrintJob CreatePrintJob(Printer printer)
        {
            if (printer.PrinterType == 1)
                return new TextPrinterJob(printer);
            if (printer.PrinterType == 2)
                return new HtmlPrinterJob(printer);
            if (printer.PrinterType == 3)
                return new PortPrinterJob(printer);
            if (printer.PrinterType == 4)
                return new DemoPrinterJob(printer);
            return new SlipPrinterJob(printer);
        }
    }
}
