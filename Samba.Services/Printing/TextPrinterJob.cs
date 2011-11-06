using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Printing
{
    public class TextPrinterJob : AbstractPrintJob
    {
        public TextPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            var q = AppServices.PrintService.GetPrinter(Printer.ShareName);
            var text = lines.Aggregate("", (current, s) => current + RemoveTag(s.Replace("|", "")) + "\r\n");
            PrintFlowDocument(q, new FlowDocument(new Paragraph(new Run(text))));
        }

        public override void DoPrint(FlowDocument document)
        {
            var q = AppServices.PrintService.GetPrinter(Printer.ShareName);
            PrintFlowDocument(q, document);
        }
    }
}
