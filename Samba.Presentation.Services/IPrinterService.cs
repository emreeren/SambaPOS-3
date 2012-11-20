using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IPrinterService : IPresentationService
    {
        IEnumerable<string> GetPrinterNames();
        void PrintTicket(Ticket ticket, PrintJob printer, Func<Order, bool> orderSelector);
        void PrintReport(FlowDocument document,Printer printer);
        void ExecutePrintJob(PrintJob printJob);
        IDictionary<string, string> GetTagDescriptions();
        IEnumerable<Printer> GetPrinters();
        IEnumerable<PrinterTemplate> GetAllPrinterTemplates();
    }
}