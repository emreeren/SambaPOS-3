using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;
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
            var fm = document.FontFamily;
            var bg = document.Background;

            var q = PrinterInfo.GetPrinter(Printer.ShareName);
            var pd = new PrintDialog { PrintQueue = q };
            if (q != null || pd.PrintQueue.FullName == Printer.ShareName || Printer.ShareName.ToLower() == "default" || Printer.ShareName.Contains("/") || pd.ShowDialog().GetValueOrDefault(false))
            {
                document.Background = Brushes.Transparent;
                document.FontFamily = new FontFamily(LocalSettings.PrintFontFamily);
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

            document.Background = bg;
            document.FontFamily = fm;
            document.PageHeight = ph;
            document.PageWidth = pw;
            document.PagePadding = pp;
            document.ColumnGap = cg;
            document.ColumnWidth = cw;
        }
    }
}
