using System;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Services.Implementations.PrinterModule.PrintJobs;

namespace Samba.Services.Implementations.PrinterModule
{
    public class TicketPrinter
    {
        private readonly Ticket _ticket;
        private PrintJob _printJob;
        private Func<Order, bool> _orderSelector;
        private bool _highPriority;
        private TicketPrintTaskBuilder _ticketPrintTaskBuilder;
        private PrinterService _printerService;
        private ILogService _logService;

        private TicketPrinter(Ticket ticket)
        {
            _ticket = ticket;
        }

        public static TicketPrinter For(Ticket ticket)
        {
            return new TicketPrinter(ticket);
        }

        public TicketPrinter WithPrintJob(PrintJob printJob)
        {
            _printJob = printJob;
            return this;
        }

        public TicketPrinter WithOrderSelector(Func<Order, bool> orderSelector)
        {
            _orderSelector = orderSelector;
            return this;
        }

        public TicketPrinter WithTaskBuilder(TicketPrintTaskBuilder ticketPrintTaskBuilder)
        {
            _ticketPrintTaskBuilder = ticketPrintTaskBuilder;
            return this;
        }

        public TicketPrinter WithPrinterService(PrinterService printerService)
        {
            _printerService = printerService;
            return this;
        }

        public TicketPrinter WithLogService(ILogService logService)
        {
            _logService = logService;
            return this;
        }

        public TicketPrinter IsHighPriority(bool highPriority)
        {
            _highPriority = highPriority;
            return this;
        }

        public void Print()
        {
            var ticket = _highPriority ? _ticket : ObjectCloner.Clone2(_ticket);
            AsyncPrintTask.Exec(_highPriority, () => InternalPrint(ticket, _printJob, _orderSelector), _logService);
        }

        private void InternalPrint(Ticket ticket, PrintJob printJob, Func<Order, bool> orderSelector)
        {
            var tasks = _ticketPrintTaskBuilder.GetPrintTasksForTicket(ticket, printJob, orderSelector);
            foreach (var ticketPrintTask in tasks.Where(x => x != null && x.Printer != null && x.Lines != null))
            {
                PrintJobFactory.CreatePrintJob(ticketPrintTask.Printer, _printerService).DoPrint(ticketPrintTask.Lines);
            }
        }
    }
}