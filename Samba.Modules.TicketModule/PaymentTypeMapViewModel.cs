using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    public class PaymentTypeMapViewModel : AbstractMapViewModel<PaymentTypeMap>
    {
        public bool DisplayAtPaymentScreen
        {
            get { return Model.DisplayAtPaymentScreen; }
            set { Model.DisplayAtPaymentScreen = value; }
        }

        public bool DisplayUnderTicket
        {
            get { return Model.DisplayUnderTicket; }
            set { Model.DisplayUnderTicket = value; }
        }
    }
}
