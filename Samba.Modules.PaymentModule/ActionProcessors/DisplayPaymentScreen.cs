using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.PaymentModule.ActionProcessors
{
    [Export(typeof(IActionProcessor))]
    class DisplayPaymentScreen : ActionProcessor
    {
        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            if (ticket != null)
            {
                ticket.PublishEvent(EventTopicNames.MakePayment);
            }
        }

        protected override object GetDefaultData()
        {
            return new object();
        }

        protected override string GetActionName()
        {
            return Resources.DisplayPaymentScreen;
        }

        protected override string GetActionKey()
        {
            return ActionNames.DisplayPaymentScreen;
        }
    }
}
