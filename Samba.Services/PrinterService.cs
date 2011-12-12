using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Printing;

namespace Samba.Services
{
    public class PrinterService : IPrinterService
    {
        private LocalPrintServer _printServer;
        private PrintQueueCollection _printers;

        internal LocalPrintServer PrintServer
        {
            get { return _printServer ?? (_printServer = new LocalPrintServer()); }
        }

        internal PrintQueueCollection Printers
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

        internal PrintQueue FindPrinterByName(string printerName)
        {
            return Printers.FirstOrDefault(x => x.FullName == printerName);
        }

        public IEnumerable<string> GetPrinterNames()
        {
            return Printers.Select(printer => printer.FullName).ToList();
        }

        public void ManualPrintTicket(Ticket ticket, PrintJob printer)
        {
            if (printer != null) TicketPrinter.ManualPrintTicket(ticket, printer);
        }

        public void AutoPrintTicket(Ticket ticket)
        {
            TicketPrinter.AutoPrintTicket(ticket);
        }

        public void PrintReport(FlowDocument document)
        {
            TicketPrinter.PrintReport(document);
        }

        public void PrintSlipReport(FlowDocument document)
        {
            TicketPrinter.PrintSlipReport(document);
        }

        public PrintQueue GetPrinter(string shareName)
        {
            return FindPrinterByName(shareName);
        }

        public void ResetCache()
        {
            _printServer = null;
        }

        public void Reset()
        {
            _printServer = null;
        }
    }
}
