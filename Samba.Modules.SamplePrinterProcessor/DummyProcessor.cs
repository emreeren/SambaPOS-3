using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.Services;
using Samba.Services.Common;

namespace Samba.Modules.SamplePrinterProcessor
{
    [Export(typeof(IPrinterProcessor))]
    class DummyProcessor : IPrinterProcessor
    {
        public string Name { get { return "DummyProcessor"; } }

        public string[] Process(Ticket ticket, IList<Order> orders, string[] formattedLines)
        {
            InteractionService.UserIntraction.DisplayPopup(string.Format("Ticket #{0}", ticket.TicketNumber), string.Format("{0} orders processed", ticket.Orders.Count));
            return null; //Module will handle printing. 
        }

        public void EditSettings()
        {
            InteractionService.UserIntraction.GiveFeedback("No settings to change");
        }
    }
}
