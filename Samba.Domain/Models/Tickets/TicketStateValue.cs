namespace Samba.Domain.Models.Tickets
{
    public class TicketStateValue
    {
        public string GroupName { get; set; }
        public string State { get; set; }
        public string StateValue { get; set; }
        public int Quantity { get; set; }

        private static TicketStateValue _default;
        public static TicketStateValue Default
        {
            get { return _default??(_default = new TicketStateValue()); }
        }
    }
}