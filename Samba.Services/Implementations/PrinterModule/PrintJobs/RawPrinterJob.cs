using System;
using System.Windows.Documents;
using System.Windows.Media;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.Formatters;
using Samba.Services.Implementations.PrinterModule.Tools;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    public class RawPrinterJob : AbstractPrintJob
    {
        public RawPrinterJob(Printer printer)
            : base(printer)
        {

        }

        public override void DoPrint(string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            RawPrinterHelper.SendStringToPrinter(Printer.ShareName, text+Environment.NewLine);
        }

        public override void DoPrint(FlowDocument document)
        {
            return;
        }
    }
}