using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export(typeof(IActionService))]
    public class ActionService : IActionService
    {
        private readonly IAutomationService _automationService;

        [ImportMany]
        public IEnumerable<IActionProcessor> ActionProcessors { get; set; }

        [ImportingConstructor]
        public ActionService(IAutomationService automationService)
        {
            _automationService = automationService;
        }

        public bool CanProcessAction(string actionType)
        {
            return ActionProcessors.Any(x => x.Handles(actionType));
        }

        public void ProcessAction(string actionType, ActionData actionData)
        {
            var actionProcessor = ActionProcessors.FirstOrDefault(x => x.Handles(actionType));
            if (actionProcessor != null) actionProcessor.Process(actionData);
        }

        public void RegisterActions()
        {
            foreach (var actionProcessor in ActionProcessors)
            {
                _automationService.RegisterActionType(actionProcessor.ActionKey, actionProcessor.ActionName, actionProcessor.DefaultData);
            }
        }
    }
}
