using System.Linq;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.Formatters;
using Samba.Services.Implementations.PrinterModule.Tools;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    public class SlipPrinterJob : AbstractPrintJob
    {
        public SlipPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            var printer = new LinePrinter(Printer.ShareName, Printer.CharsPerLine, Printer.CodePage);
            printer.StartDocument();

            var formatters = new FormattedDocument(lines, Printer.CharsPerLine).GetFormatters().ToList();
            foreach (var formatter in formatters)
            {
                SendToPrinter(printer, formatter);
            }
            if (formatters.Count() > 1)
                printer.Cut();
            printer.EndDocument();
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document, Printer.CharsPerLine));
        }

        private static void SendToPrinter(LinePrinter printer, ILineFormatter line)
        {
            var data = line.GetFormattedLine();

            if (!data.StartsWith("<"))
                printer.WriteLine(data, line.FontHeight, line.FontWidth, LineAlignment.Left);
            else if (line.Tag.TagName == "eb")
                printer.EnableBold();
            else if (line.Tag.TagName == "db")
                printer.DisableBold();
            else if (line.Tag.TagName == "ec")
                printer.EnableCenter();
            else if (line.Tag.TagName == "el")
                printer.EnableLeft();
            else if (line.Tag.TagName == "er")
                printer.EnableRight();
            else if (line.Tag.TagName == "bmp")
                printer.PrintBitmap(RemoveTag(data));
            else if (line.Tag.TagName == "qr")
                printer.PrintQrCode(RemoveTag(data), line.FontHeight, line.FontWidth);
            else if (line.Tag.TagName == "bar")
                printer.PrintBarCode(RemoveTag(data), line.FontHeight, line.FontWidth);
            else if (line.Tag.TagName == "cut")
                printer.Cut();
            else if (line.Tag.TagName == "beep")
                printer.Beep();
            else if (line.Tag.TagName == "drawer")
                printer.OpenCashDrawer();
            else if (line.Tag.TagName == "b")
                printer.Beep((char)line.FontHeight, (char)line.FontWidth);
            else if (line.Tag.TagName == ("xct"))
                printer.ExecCommand(RemoveTag(data));
        }
    }
}
