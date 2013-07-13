using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class MoveTaggedOrders : ActionType
    {
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public MoveTaggedOrders(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            var orderTagName = actionData.GetAsString("OrderTagName");
            if (ticket != null && !string.IsNullOrEmpty(orderTagName))
            {
                var orderTagValue = actionData.GetAsString("OrderTagValue");
                if (ticket.Orders.Any(y => y.OrderTagExists(z => z.TagName == orderTagName && z.TagValue == orderTagValue)))
                {
                    var tid = ticket.Id;
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested, true);
                    ticket = _ticketService.OpenTicket(tid);
                    var orders = ticket.Orders.Where(y => y.OrderTagExists(z => z.TagName == orderTagName && z.TagValue == orderTagValue)).ToArray();
                    var commitResult = _ticketService.MoveOrders(ticket, orders, 0);
                    if (string.IsNullOrEmpty(commitResult.ErrorMessage) && commitResult.TicketId > 0)
                    {
                        ExtensionMethods.PublishIdEvent(commitResult.TicketId, EventTopicNames.DisplayTicket);
                    }
                    else
                    {
                        ExtensionMethods.PublishIdEvent(tid, EventTopicNames.DisplayTicket);
                    }
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { OrderTagName = "", OrderTagValue = "" };
        }

        protected override string GetActionName()
        {
            return Resources.MoveTaggedOrders;
        }

        protected override string GetActionKey()
        {
            return ActionNames.MoveTaggedOrders;
        }
    }
}
