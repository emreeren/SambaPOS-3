using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule
{
    public class TicketPrintTask
    {
        public Printer Printer { get; set; }
        public string[] Lines { get; set; }
    }
}