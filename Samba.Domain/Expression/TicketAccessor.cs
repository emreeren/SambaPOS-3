using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Expression
{
    public static class TicketAccessor
    {
        private static Ticket _model;
        public static Ticket Model
        {
            get { return _model ?? (_model = Ticket.Empty); }
            set { _model = value; }
        }

        public static int OrderCount { get { return Model.Orders.Count; } }
        public static decimal RemainingAmount { get { return Model.GetRemainingAmount(); } }
        public static void RemovePayments(List<object> items)
        {
            items.Where(x => x is Payment).Cast<Payment>().ToList().ForEach(x => Model.Payments.Remove(x));
        }
    }
}