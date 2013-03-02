﻿using System.Windows;
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
            var text = new FormattedDocument(lines, Printer.CharsPerLine).GetFormattedText();
            DoPrint(new FlowDocument(new Paragraph(new Run(text))));
        }

        public override void DoPrint(FlowDocument document)
        {
            var ph = document.PageHeight;
            var pw = document.PageWidth;
            var pp = document.PagePadding;
            var cg = document.ColumnGap;
            var cw = document.ColumnWidth;

            var q = PrinterInfo.GetPrinter(Printer.ShareName);
            var pd = new PrintDialog { PrintQueue = q };
            if (pd.PrintQueue.FullName == Printer.ShareName || pd.ShowDialog().GetValueOrDefault(false))
            {
                document.PageHeight = pd.PrintableAreaHeight;
                document.PageWidth = pd.PrintableAreaWidth;
                document.PagePadding = new Thickness(25);
                document.ColumnGap = 0;
                document.ColumnWidth = (document.PageWidth -
                                       document.ColumnGap -
                                       document.PagePadding.Left -
                                       document.PagePadding.Right);
                pd.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "");
            }

            document.PageHeight = ph;
            document.PageWidth = pw;
            document.PagePadding = pp;
            document.ColumnGap = cg;
            document.ColumnWidth = cw;
        }
    }
}
