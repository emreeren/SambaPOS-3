using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class PayTicket : ActionType
    {
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PayTicket(ITicketService ticketService, ICacheService cacheService)
        {
            _ticketService = ticketService;
            _cacheService = cacheService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");

            if (ticket != null && ticket != Ticket.Empty && _ticketService.CanSettleTicket(ticket))
            {
                var paymentTypeName = actionData.GetAsString("PaymentTypeName");
                var paymentType = _cacheService.GetPaymentTypeByName(paymentTypeName);
                if (paymentType != null && ticket.RemainingAmount > 0)
                {
                    _ticketService.PayTicket(ticket, paymentType);
                }
                actionData.DataObject.RemainingAmount = 0m;
            }
        }

        protected override object GetDefaultData()
        {
            return new { PaymentTypeName = string.Empty };
        }

        protected override string GetActionName()
        {
            return Resources.PayTicket;
        }

        protected override string GetActionKey()
        {
            return "PayTicket";
        }
    }

}