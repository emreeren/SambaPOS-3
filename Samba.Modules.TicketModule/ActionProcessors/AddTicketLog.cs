using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class AddTicketLog : ActionType
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public AddTicketLog(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            if (ticket != null)
            {
                var category = actionData.GetAsString("Category");
                var log = actionData.GetAsString("Log");
                if (!string.IsNullOrEmpty(log))
                {
                    ticket.AddLog(_applicationState.CurrentLoggedInUser.Name, category, log);
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { Category = "", Log = "" };
        }

        protected override string GetActionName()
        {
            return Resources.AddTicketLog;
        }

        protected override string GetActionKey()
        {
            return "AddTicketLog";
        }
    }
}
