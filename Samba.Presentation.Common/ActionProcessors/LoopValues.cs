using System.ComponentModel.Composition;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class LoopValues : ActionType
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public LoopValues(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        public override void Process(ActionData actionData)
        {
            var name = actionData.GetAsString("Name");
            var values = actionData.GetAsString("Values");
            if (!string.IsNullOrEmpty(values))
            {
                foreach (var value in values.Split(','))
                {
                    _applicationState.NotifyEvent(RuleEventNames.ValueLooped, new { Name = name, Value = value });
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { Name = "", Values = "" };
        }

        protected override string GetActionName()
        {
            return Resources.LoopValues;
        }

        protected override string GetActionKey()
        {
            return ActionNames.LoopValues;
        }
    }
}
