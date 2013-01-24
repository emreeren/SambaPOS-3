using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Persistance;
using Samba.Presentation.Services;

namespace Samba.Modules.TicketModule
{
    [Export]
    public class TicketModuleViewModel
    {
        private readonly ITicketService _ticketService;
        private readonly IApplicationState _applicationState;
        private readonly IList<ITicketExplorerFilter> _emptyFilters = new List<ITicketExplorerFilter>();

        [ImportingConstructor]
        public TicketModuleViewModel(ITicketService ticketService, IApplicationState applicationState)
        {
            _ticketService = ticketService;
            _applicationState = applicationState;
        }

        private IEnumerable<Ticket> _tickets;
        public IEnumerable<Ticket> Tickets
        {
            get { return _tickets ?? (_tickets = GetTickets()); }
        }

        private IEnumerable<Ticket> GetTickets()
        {
            var wp = _applicationState.CurrentWorkPeriod;
            if (wp == null) return new List<Ticket>();
            return _ticketService.GetFilteredTickets(wp.StartDate, wp.EndDate, _emptyFilters).OrderByDescending(x => x.Date);
        }
    }
}
