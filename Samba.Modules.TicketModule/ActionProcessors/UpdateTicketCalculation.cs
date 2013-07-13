using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class UpdateTicketCalculation : ActionType
    {
        private readonly ICacheService _cacheService;
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public UpdateTicketCalculation(ICacheService cacheService, ITicketService ticketService)
        {
            _cacheService = cacheService;
            _ticketService = ticketService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            if (ticket != null)
            {
                var calculationTypeName = actionData.GetAsString("CalculationType");
                var calculationType = _cacheService.GetCalculationTypeByName(calculationTypeName);
                if (calculationType != null)
                {
                    var amount = actionData.GetAsDecimal("Amount");
                    ticket.AddCalculation(calculationType, amount);
                    _ticketService.RecalculateTicket(ticket);
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.RegenerateSelectedTicket);
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { CalculationType = "", Amount = 0m };
        }

        protected override string GetActionName()
        {
            return Resources.UpdateTicketCalculation;
        }

        protected override string GetActionKey()
        {
            return ActionNames.UpdateTicketCalculation;
        }
    }
}
