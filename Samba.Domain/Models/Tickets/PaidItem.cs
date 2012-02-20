using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaidItem:Value
    {
        public int MenuItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
