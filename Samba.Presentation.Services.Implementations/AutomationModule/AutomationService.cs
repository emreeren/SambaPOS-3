using System.ComponentModel.Composition;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.AutomationModule
{
    [Export(typeof(IAutomationService))]
    class AutomationService : IAutomationService
    {
        private readonly IAutomationServiceBase _automationServiceBase;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public AutomationService(IAutomationServiceBase automationServiceBase, IApplicationState applicationState)
        {
            _automationServiceBase = automationServiceBase;
            _applicationState = applicationState;
        }

        public void NotifyEvent(string eventName, object dataObject)
        {
            var terminalId = _applicationState.CurrentTerminal.Id;
            var departmentId = _applicationState.CurrentDepartment.Id;
            var roleId = _applicationState.CurrentLoggedInUser.UserRole.Id;

            _automationServiceBase.NotifyEvent(eventName, dataObject, terminalId, departmentId, roleId, x => x.PublishEvent(EventTopicNames.ExecuteEvent, true));
        }
    }
}
