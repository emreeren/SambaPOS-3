using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))] //MEF
    class PayTicket : ActionType
    {
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor] //MEF 
        public PayTicket(ITicketService ticketService, ICacheService cacheService)
        {
            _ticketService = ticketService;
            _cacheService = cacheService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");

            if (ticket != null)
            {
                var paymentTypeName = actionData.GetAsString("PaymentTypeName");
                var paymentType = _cacheService.GetPaymentTypeByName(paymentTypeName);
                if (paymentType != null && ticket.RemainingAmount > 0)
                {
                    _ticketService.PayTicket(ticket, paymentType);
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { PaymentTypeName = string.Empty };
        }

        protected override string GetActionName()
        {
            return "Pay Ticket";
        }

        protected override string GetActionKey()
        {
            return "PayTicket";
        }
    }

}