using System;

namespace Samba.Domain.Models.Tickets
{
    public class Payment
    {
        public Payment()
        {
            Date = DateTime.Now;
            PaymentType = 0;
        }

        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int PaymentType { get; set; }
        public int UserId { get; set; }
    }
}
