using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
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
        IEnumerable<string> GetCustomPrinterNames();
        ICustomPrinter GetCustomPrinter(string customPrinterName);
        void PrintTicket(Ticket ticket, PrintJob printer, Func<Order, bool> orderSelector);
        void PrintAccountTransactionDocument(AccountTransactionDocument document, Printer printer, PrinterTemplate printerTemplate);
        void PrintEntity(Entity entity, Printer printer, PrinterTemplate printerTemplate);
        void PrintReport(FlowDocument document, Printer printer);
        void ExecutePrintJob(PrintJob printJob);
        IDictionary<string, string> GetTagDescriptions();
        void ResetCache();
        string GetPrintingContent(Ticket ticket, string format, int width);
        object GetCustomPrinterData(string customPrinterName, string customPrinterData);
    }
}