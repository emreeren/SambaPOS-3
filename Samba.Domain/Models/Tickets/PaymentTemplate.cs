using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaymentTemplate : Entity, IOrderable
    {
        public PaymentTemplate()
        {
            ButtonColor = "Gainsboro";
            DisplayAtPaymentScreen = true;
        }

        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public string ButtonColor { get; set; }
        public bool DisplayAtPaymentScreen { get; set; }
        public bool DisplayUnderTicket { get; set; }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
        public virtual Account Account { get; set; }
    }
}
