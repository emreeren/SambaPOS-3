using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Actions;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.AutomationModule.ServiceImplementations
{
    [Export(typeof(IRuleService))]
    class RuleService : AbstractService, IRuleService
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public RuleService(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = Dao.Query<AppRule>(x => x.Actions)); } }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = Dao.Query<AppAction>()); } }

        public void NotifyEvent(string eventName, object dataObject)
        {
            var rules = Rules.Where(x => x.EventName == eventName);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                foreach (var actionContainer in rule.Actions)
                {
                    var container = actionContainer;
                    var action = Actions.SingleOrDefault(x => x.Id == container.AppActionId);

                    if (action != null)
                    {
                        IActionData data = new ActionData { Action = action, DataObject = dataObject, ParameterValues = container.ParameterValues };
                        data.PublishEvent(EventTopicNames.ExecuteEvent, true);
                    }
                }
            }
        }

        private bool SatisfiesConditions(AppRule appRule, object dataObject)
        {
            var conditions = appRule.EventConstraints.Split('#')
                .Select(x => new RuleConstraintViewModel(x));

            var parameterNames = dataObject.GetType().GetProperties().Select(x => x.Name);

            foreach (var condition in conditions)
            {
                var parameterName = parameterNames.FirstOrDefault(condition.Name.Equals);

                if (!string.IsNullOrEmpty(parameterName))
                {
                    var property = dataObject.GetType().GetProperty(parameterName);
                    var parameterValue = property.GetValue(dataObject, null) ?? "";
                    if (!condition.ValueEquals(parameterValue)) return false;
                }
                else
                {
                    if (condition.Name.StartsWith("SN$"))
                    {
                        var settingName = condition.Name.Replace("SN$", "");
                        var settingValue = condition.Value;
                        if (AppServices.SettingService.GetCustomSetting(settingName).StringValue != settingValue)
                            return false;
                    }
                    if (condition.Name == "TerminalName" && !string.IsNullOrEmpty(condition.Value))
                    {
                        if (!condition.Value.Equals(AppServices.CurrentTerminal.Name))
                        {
                            return false;
                        }
                    }
                    if (condition.Name == "DepartmentName" && !string.IsNullOrEmpty(condition.Value))
                    {
                        if (_applicationState.CurrentDepartment == null ||
                            !condition.Value.Equals(_applicationState.CurrentDepartment.Name))
                        {
                            return false;
                        }
                    }

                    if (condition.Name == "UserName" && !string.IsNullOrEmpty(condition.Value))
                    {
                        if (_applicationState.CurrentLoggedInUser == null ||
                            !condition.Value.Equals(_applicationState.CurrentLoggedInUser.Name))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public override void Reset()
        {
            _rules = null;
            _actions = null;
        }
    }
}
