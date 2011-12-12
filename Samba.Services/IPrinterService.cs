using System.Collections.Generic;
using System.Printing;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface IPrinterService : IService
    {
        IEnumerable<string> GetPrinterNames();
        void ManualPrintTicket(Ticket ticket, PrintJob printer);
        void AutoPrintTicket(Ticket ticket);
        void PrintReport(FlowDocument document);
        void PrintSlipReport(FlowDocument document);
        PrintQueue GetPrinter(string shareName);
        void ResetCache();
    }
}