using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaidItem : Value
    {
        public string Key { get; set; }
        public decimal Quantity { get; set; }
        public int TicketId { get; set; }
    }
}
