using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionProcessor))]
    class SendMessage : ActionProcessor
    {
        private readonly IMessagingService _messagingService;

        [ImportingConstructor]
        public SendMessage(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public override void Process(ActionData actionData)
        {
            _messagingService.SendMessage("ActionMessage", actionData.GetAsString("Command"));
        }

        protected override object GetDefaultData()
        {
            return new { Command = "" };
        }

        protected override string GetActionName()
        {
            return Resources.BroadcastMessage;
        }

        protected override string GetActionKey()
        {
            return ActionNames.SendMessage;
        }
    }
}
