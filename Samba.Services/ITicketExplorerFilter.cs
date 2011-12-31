using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public enum FilterType
    {
        OpenTickets,
        AllTickets,
        Account,
        Location
    }

    public interface ITicketExplorerFilter
    {
        int FilterTypeIndex { get; set; }
        bool IsTextBoxEnabled { get; }
        string[] FilterTypes { get; }
        FilterType FilterType { get; set; }
        string FilterValue { get; set; }
        List<string> FilterValues { get; set; }
        Expression<Func<Ticket, bool>> GetExpression();
    }
}