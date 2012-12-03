using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Services.Implementations.PrinterModule.Formatters;
using Samba.Presentation.Services.Implementations.PrinterModule.Tools;

namespace Samba.Presentation.Services.Implementations.PrinterModule.PrintJobs
{
    public class TextPrinterJob : AbstractPrintJob
    {
        public TextPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            var q = PrinterInfo.GetPrinter(Printer.ShareName);
            var text = new FormattedDocument(lines, Printer.CharsPerLine).GetFormattedText();
            PrintFlowDocument(q, new FlowDocument(new Paragraph(new Run(text))));
        }

        public override void DoPrint(FlowDocument document)
        {
            var q = PrinterInfo.GetPrinter(Printer.ShareName);
            PrintFlowDocument(q, document);
        }
    }
}
