using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Web.Http;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.DaoClasses;

namespace Samba.ApiServer.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TicketsController : ApiController
    {
        private readonly ITicketDao _ticketDao;

        [ImportingConstructor]
        public TicketsController(ITicketDao ticketDao)
        {
            _ticketDao = ticketDao;
        }

        public IEnumerable<Ticket> GetAllTickets()
        {
            return _ticketDao.GetAllTickets();
        }

        public Ticket GetTicketById(int id)
        {
            return _ticketDao.GetTicketById(id);
        }
    }
}
