using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface ITicketDao
    {
        TicketCommitResult CheckConcurrency(Ticket ticket);
        void Save(Ticket ticket);
        Ticket OpenTicket(int ticketId);
        int GetOpenTicketCount();
        IEnumerable<int> GetOpenTicketIds(int resourceId);
        IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction);
        IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters);
        IEnumerable<Order> GetOrders(int ticketId);
        void SaveFreeOrderTag(int orderTagGroupId, OrderTag orderTag);
        void SaveFreeTicketTag(int ticketTagGroupId, string freeTag);
    }
}
