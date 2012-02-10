using System;
using Samba.Domain.Models.Accounts;

namespace Samba.Domain.Models.Tickets
{
    public enum DiscountType
    {
        Percent,
        Amount,
        Rounding
    }

    public class Discount
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DiscountType { get; set; }
        public decimal Value { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
