namespace Samba.Domain.Models.Tickets
{
    public class PaidItem
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
