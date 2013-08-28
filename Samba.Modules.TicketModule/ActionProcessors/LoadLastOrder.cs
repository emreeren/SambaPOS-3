using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    class LoadLastOrder : ActionType
    {
        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            if (ticket != null && ticket.Orders.Count > 0)
            {
                actionData.DataObject.Order = ticket.Orders.Last();
            }
        }

        protected override object GetDefaultData()
        {
            return new object();
        }

        protected override string GetActionName()
        {
            return Resources.LoadLastOrder;
        }

        protected override string GetActionKey()
        {
            return "LoadLastOrder";
        }
    }
}
