using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Helpers;
using Samba.Services.Implementations.PrinterModule.Formatters;
using Samba.Services.Implementations.PrinterModule.Tools;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    class DemoPrinterJob : AbstractPrintJob
    {
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, byte[] lParam);
        [DllImport("User32.dll")]
        public static extern int SendMessageW(IntPtr hWnd, int uMsg, int wParam, byte[] lParam);

        public DemoPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            Debug.Assert(!string.IsNullOrEmpty(Printer.ShareName));
            var text = new FormattedDocument(lines, Printer.CharsPerLine).GetFormattedText();
            if (!Utility.IsValidFile(Printer.ShareName) || !SaveToFile(Printer.ShareName, text))
                SendToNotepad(Printer, text);
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document,Printer.CharsPerLine));
        }

        private static void SendToNotepad(Printer printer, string text)
        {
            var pcs = printer.ShareName.Split('#');
            var wname = "Edit";
            if (pcs.Length > 1) wname = pcs[1];

            var notepads = Process.GetProcessesByName(pcs[0]);

            if (notepads.Length == 0)
                notepads = Process.GetProcessesByName("notepad");

            if (notepads.Length == 0) return;

            if (notepads[0] != null)
            {
                IntPtr child = FindWindowEx(notepads[0].MainWindowHandle, new IntPtr(0), wname, null);
                var bytes = Encoding.Unicode.GetBytes(text);
                SendMessageW(child, 0x000C, bytes.Length, bytes);
            }
        }

        private static bool SaveToFile(string fileName, string text)
        {
            try
            {
                File.WriteAllText(fileName, text);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
