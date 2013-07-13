using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.PosModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class DisplayTicketList : ActionType
    {
        public override void Process(ActionData actionData)
        {
            var ticketTagName = actionData.GetAsString("TicketTagName");
            var ticketStateName = actionData.GetAsString("TicketStateName");

            if (!string.IsNullOrEmpty(ticketStateName))
            {
                var dt = new TicketStateData { StateName = ticketStateName };
                dt.PublishEvent(EventTopicNames.ActivateTicketList);
            }
            else if (!string.IsNullOrEmpty(ticketTagName))
            {
                var dt = new TicketTagGroup { Name = ticketTagName };
                dt.PublishEvent(EventTopicNames.ActivateTicketList);
            }
        }

        protected override object GetDefaultData()
        {
            return new { TicketTagName = "", TicketStateName = "" };
        }

        protected override string GetActionName()
        {
            return Resources.DisplayTicketList;
        }

        protected override string GetActionKey()
        {
            return ActionNames.DisplayTicketList;
        }
    }
}
