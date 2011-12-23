using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Modules.PrinterModule.Formatters;
using Samba.Modules.PrinterModule.Tools;

namespace Samba.Modules.PrinterModule.PrintJobs
{
    class DemoPrinterJob : AbstractPrintJob
    {
        public DemoPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            Debug.Assert(!string.IsNullOrEmpty(Printer.ShareName));
            var pcs = Printer.ShareName.Split('#');
            var wname = "Edit";
            if (pcs.Length > 1)
                wname = pcs[1];

            var notepads = Process.GetProcessesByName(pcs[0]);

            if (notepads.Length == 0)
                notepads = Process.GetProcessesByName("notepad");

            if (notepads.Length == 0) return;

            if (notepads[0] != null)
            {
                IntPtr child = NativeMethods.FindWindowEx(notepads[0].MainWindowHandle, new IntPtr(0), wname, null);
                var text = new FormattedDocument(lines, Printer.CharsPerLine).GetFormattedText();
                NativeMethods.SendMessage(child, 0x000C, 0, text);
            }
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document));
        }
    }
}
