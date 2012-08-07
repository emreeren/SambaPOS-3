using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class PaymentTemplateMapViewModel : AbstractMapViewModel<PaymentTemplateMap>
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
