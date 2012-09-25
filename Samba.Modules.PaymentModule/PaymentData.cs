using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PaymentModule
{
    public class PaymentData
    {
        public PaymentTemplate PaymentTemplate { get; set; }
        public ChangePaymentTemplate ChangePaymentTemplate { get; set; }
        public decimal PaymentDueAmount { get; set; }
        public decimal TenderedAmount { get; set; }
    }
}