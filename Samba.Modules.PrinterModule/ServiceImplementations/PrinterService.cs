using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Printing;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.PrinterModule.ServiceImplementations
{
    [Export(typeof(IPrinterService))]
    public class PrinterService : AbstractService, IPrinterService
    {
        public IEnumerable<string> GetPrinterNames()
        {
            return PrinterInfo.GetPrinterNames();
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
            return PrinterInfo.GetPrinter(shareName);
        }

        public override void Reset()
        {
            PrinterInfo.ResetCache();
        }
    }
}
