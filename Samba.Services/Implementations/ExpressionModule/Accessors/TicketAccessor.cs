using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.ExpressionModule.Accessors
{
    public static class TicketAccessor
    {
        private static Ticket _model;
        public static Ticket Model
        {
            get { return _model ?? (_model = Ticket.Empty); }
            set { _model = value; }
        }

        public static int Id { get { return Model.Id; } }
        public static int OrderCount { get { return Model.Orders.Count; } }
        public static decimal RemainingAmount { get { return Model.GetRemainingAmount(); } }
        public static void RemovePayments(List<object> items)
        {
            items.Where(x => x is Payment).Cast<Payment>().ToList().ForEach(x => Model.Payments.Remove(x));
        }
        public static string CreatingUserName { get { return Model.Orders.Any() ? Model.Orders[0].CreatingUserName : ""; } }
        public static string LastUserName { get { return Model.Orders.Any() ? Model.Orders.OrderBy(x => x.Id).Last().CreatingUserName : ""; } }
        public static bool IsInState(string state)
        {
            if (state.Contains(":"))
            {
                var parts = state.Split(new[] { ':' }, 2);
                return InState(parts[0], parts[1]);
            }
            return Model.IsInState("*", state);
        }

        public static bool InState(string stateName, string state)
        {
            return Model.IsInState(stateName, state);
        }
    }
}