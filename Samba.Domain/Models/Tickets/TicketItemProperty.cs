namespace Samba.Domain.Models.Tickets
{
    public class TicketItemProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TicketItemId { get; set; }
        public decimal Price { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Quantity { get; set; }
        public int PropertyGroupId { get; set; }
        public int MenuItemId { get; set; }
        public bool CalculateWithParentPrice { get; set; }
        public string PortionName { get; set; }
    }
}
