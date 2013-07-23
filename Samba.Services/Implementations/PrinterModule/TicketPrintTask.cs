using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule
{
    internal class TicketPrintTask
    {
        public Printer Printer { get; set; }
        public string[] Lines { get; set; }
    }
}