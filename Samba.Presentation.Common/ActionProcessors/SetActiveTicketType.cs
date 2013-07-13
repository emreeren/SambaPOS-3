using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class SetActiveTicketType : ActionType
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public SetActiveTicketType(ICacheService cacheService, IApplicationState applicationState)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
        }

        public override void Process(ActionData actionData)
        {
            var ticketTypeName = actionData.GetAsString("TicketTypeName");
            var ticketType = _cacheService.GetTicketTypes().SingleOrDefault(y => y.Name == ticketTypeName);
            if (ticketType != null)
            {
                _applicationState.TempTicketType = ticketType;
            }
            else if (_applicationState.SelectedEntityScreen != null && _applicationState.SelectedEntityScreen.TicketTypeId != 0)
            {
                _applicationState.TempTicketType = _cacheService.GetTicketTypeById(_applicationState.SelectedEntityScreen.TicketTypeId);
            }
            else
            {
                _applicationState.TempTicketType = _cacheService.GetTicketTypeById(_applicationState.CurrentDepartment.TicketTypeId);
            }
        }

        protected override object GetDefaultData()
        {
            return new {TicketTypeName = ""};
        }

        protected override string GetActionName()
        {
            return Resources.SetActiveTicketType;
        }

        protected override string GetActionKey()
        {
            return ActionNames.SetActiveTicketType;
        }
    }
}
