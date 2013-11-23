using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Threading;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IPrinterService
    {
        IEnumerable<string> GetPrinterNames();
        IEnumerable<string> GetCustomPrinterNames();
        ICustomPrinter GetCustomPrinter(string customPrinterName);
        void PrintTicket(Ticket ticket, PrintJob printer, Func<Order, bool> orderSelector, bool highPriority);
        void PrintObject(object item, Printer printer, PrinterTemplate printerTemplate);
        void PrintReport(FlowDocument document, Printer printer);
        void ExecutePrintJob(PrintJob printJob, bool highPriority);
        IDictionary<string, string> GetTagDescriptions();
        void ResetCache();
        string GetPrintingContent(Ticket ticket, string format, int width);
        string ExecuteFunctions<T>(string printerTemplate, T model);
        object GetCustomPrinterData(string customPrinterName, string customPrinterData);
    }
}