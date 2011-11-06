namespace Samba.Domain.Models.Tickets
{
    public enum DiscountType
    {
        Percent,
        Amount,
        Auto
    }

    public class Discount
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TicketItemId { get; set; }
        public int DiscountType { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
