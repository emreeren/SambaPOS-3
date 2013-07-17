using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Common;

namespace Samba.Services
{
    public interface ITicketServiceBase
    {
        IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters);
        IList<ITicketExplorerFilter> CreateTicketExplorerFilters();
        IEnumerable<Order> GetOrders(int ticketId);
        IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction);
        IEnumerable<OpenTicketData> GetOpenTickets(int entityId);
        IEnumerable<int> GetOpenTicketIds(int entityId);
        int GetOpenTicketCount();
        void UpdateAccountOfOpenTickets(Entity entity);
        IEnumerable<Ticket> GetTicketsByState(string state);
    }
}
