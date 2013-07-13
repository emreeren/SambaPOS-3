using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionProcessor))]
    class CloseActiveTicket : ActionProcessor
    {
        public override void Process(ActionData actionData)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested, true);
        }

        protected override object GetDefaultData()
        {
            return new object();
        }

        protected override string GetActionName()
        {
            return Resources.CloseTicket;
        }

        protected override string GetActionKey()
        {
            return ActionNames.CloseActiveTicket;
        }
    }
}
