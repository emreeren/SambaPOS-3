using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class DisplayTicket : ActionType
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public DisplayTicket(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        public override void Process(ActionData actionData)
        {
            var ticketId = actionData.GetAsInteger("TicketId");
            if (ticketId > 0)
                ExtensionMethods.PublishIdEvent(ticketId, EventTopicNames.DisplayTicket);
            else
            {
                if (_applicationState.IsLocked)
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
            }
        }

        protected override object GetDefaultData()
        {
            return new { TicketId = 0 };
        }

        protected override string GetActionName()
        {
            return Resources.DisplayTicket;
        }

        protected override string GetActionKey()
        {
            return ActionNames.DisplayTicket;
        }
    }
}
