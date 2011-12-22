using System.Linq;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Modules.PrinterModule.Tools;
using Samba.Services;

namespace Samba.Modules.PrinterModule
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
            var text = lines.Aggregate("", (current, s) => current + RemoveTag(s.Replace("|", "")) + "\r\n");
            PrintFlowDocument(q, new FlowDocument(new Paragraph(new Run(text))));
        }

        public override void DoPrint(FlowDocument document)
        {
            var q = PrinterInfo.GetPrinter(Printer.ShareName);
            PrintFlowDocument(q, document);
        }
    }
}
