using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Services.Implementations.PrinterModule.Formatters;
using Samba.Presentation.Services.Implementations.PrinterModule.Tools;

namespace Samba.Presentation.Services.Implementations.PrinterModule.PrintJobs
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

            var formatters = new FormattedDocument(lines, Printer.CharsPerLine).GetFormatters();
            foreach (var formatter in formatters)
            {
                SendToPrinter(printer, formatter);
            }

            printer.Cut();
            printer.EndDocument();
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document));
        }

        private static void SendToPrinter(LinePrinter printer, ILineFormatter line)
        {
            var data = line.GetFormattedLine();

            if (!data.StartsWith("<"))
                printer.WriteLine(data, line.FontHeight, line.FontWidth, LineAlignment.Left);
            else if (line.Tag.TagName == "eb")
                printer.EnableBold();
            else if (line.Tag.TagName == ("db"))
                printer.DisableBold();
            else if (line.Tag.TagName == "bmp")
                printer.PrintBitmap(RemoveTag(data));
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
