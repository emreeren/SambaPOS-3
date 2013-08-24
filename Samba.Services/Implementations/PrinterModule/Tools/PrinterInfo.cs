using System.Collections.Generic;
using System.Linq;
using System.Printing;

namespace Samba.Services.Implementations.PrinterModule.Tools
{
    internal static class PrinterInfo
    {
        private static LocalPrintServer _printServer;
        internal static LocalPrintServer PrintServer
        {
            get { return _printServer ?? (_printServer = new LocalPrintServer()); }
        }

        private static PrintQueueCollection _printers;
        internal static PrintQueueCollection Printers
        {
            get
            {
                return _printers ?? (_printers = PrintServer.GetPrintQueues(new[]
                                                                            {
                                                                                EnumeratedPrintQueueTypes.Local,
                                                                                EnumeratedPrintQueueTypes.Connections
                                                                            }));
            }
        }

        public static PrintQueue GetPrinter(string shareName)
        {
            var result = FindPrinterByName(shareName);
            if (result == null) result = FindPrinterByShareName(shareName);
            return result;
        }

        public static PrintQueue FindPrinterByShareName(string shareName)
        {
            return Printers.FirstOrDefault(x => x.HostingPrintServer.Name +"\\"+ x.ShareName == shareName);
        }

        internal static PrintQueue FindPrinterByName(string printerName)
        {
            return Printers.FirstOrDefault(x => x.FullName == printerName);
        }

        public static IEnumerable<string> GetPrinterNames()
        {
            return Printers.Select(printer => printer.FullName).ToList();
        }

        public static void ResetCache()
        {
            _printServer = null;
        }
    }
}
