using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Common;

namespace Samba.Persistance
{
    public interface ITicketDao
    {
        TicketCommitResult CheckConcurrency(Ticket ticket);
        void Save(Ticket ticket);
        Ticket OpenTicket(int ticketId);
        int GetOpenTicketCount();
        IEnumerable<int> GetOpenTicketIds(int entityId);
        IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction);
        IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters);
        IEnumerable<Order> GetOrders(int ticketId);
        void SaveFreeOrderTag(int orderTagGroupId, OrderTag orderTag);
        void SaveFreeTicketTag(int ticketTagGroupId, string freeTag);
        IEnumerable<Ticket> GetAllTickets();
        Ticket GetTicketById(int id);
    }
}
