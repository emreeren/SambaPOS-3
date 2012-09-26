using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.Formatters;
using Samba.Services.Implementations.PrinterModule.Tools;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    class WindowsPrinterJob : AbstractPrintJob
    {
        public WindowsPrinterJob(Printer printer)
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
            var pd = new PrintDialog();
            if (pd.ShowDialog().GetValueOrDefault(false))
                pd.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "");
        }
    }
}
