using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IPrinterService 
    {
        IEnumerable<string> GetPrinterNames();
        IEnumerable<string> GetProcessorNames();
        IPrinterProcessor GetPrinterProcessor(string processorName);
        void PrintTicket(Ticket ticket, PrintJob printer, Func<Order, bool> orderSelector);
        void PrintReport(FlowDocument document,Printer printer);
        void ExecutePrintJob(PrintJob printJob);
        IDictionary<string, string> GetTagDescriptions();
        void ResetCache();
        string GetPrintingContent(Ticket ticket, string format,int width);
        string GetDefaultTicketPrintTemplate();
        string GetDefaultKitchenPrintTemplate();
    }
}