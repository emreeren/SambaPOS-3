﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;

namespace Samba.Persistance
{
    public interface ITicketExplorerFilter
    {
        bool IsTextBoxEnabled { get; }
        IEnumerable<string> FilterTypes { get; }
        string FilterType { get; set; }
        string FilterValue { get; set; }
        List<string> FilterValues { get; set; }
        Expression<Func<Ticket, bool>> GetExpression();
    }
}