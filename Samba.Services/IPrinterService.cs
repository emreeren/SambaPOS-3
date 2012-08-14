using System.Collections.Generic;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IPrinterService : IService
    {
        IEnumerable<string> GetPrinterNames();
        void PrintTicket(Ticket ticket, PrintJob printer);
        void PrintReport(FlowDocument document);
        void PrintSlipReport(FlowDocument document);
        void ExecutePrintJob(PrintJob printJob);
        IDictionary<string, string> GetTagDescriptions();
        IEnumerable<Printer> GetPrinters();
        IEnumerable<PrinterTemplate> GetAllPrinterTemplates();
    }
}