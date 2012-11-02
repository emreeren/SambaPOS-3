using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface IExpressionService
    {
        string Eval(string expression);
        bool EvalTicketCommand(string canExecuteScript, Ticket selectedTicket);
    }
}
