using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.AutomationModule.Accessors
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
    }
}