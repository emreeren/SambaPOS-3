using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Actions;
using Samba.Infrastructure.Data.Serializer;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export(typeof(IAutomationService))]
    class AutomationService : AbstractService, IAutomationService
    {
        private readonly IApplicationState _applicationState;
        private readonly RuleActionTypeRegistry _ruleActionTypeRegistry;
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public AutomationService(IApplicationState applicationState, ISettingService settingService)
        {
            _applicationState = applicationState;
            _ruleActionTypeRegistry = new RuleActionTypeRegistry();
            _settingService = settingService;
        }

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = Dao.Query<AppRule>(x => x.Actions)); } }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = Dao.Query<AppAction>()); } }

        public void NotifyEvent(string eventName, object dataObject)
        {
            var settingReplacer = _settingService.GetSettingReplacer();
            var rules = Rules.Where(x => x.EventName == eventName);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                foreach (var actionContainer in rule.Actions)
                {
                    var container = actionContainer;
                    var action = Actions.SingleOrDefault(x => x.Id == container.AppActionId);

                    if (action != null)
                    {
                        //IActionData data = new ActionData { Action = action, DataObject = dataObject, ParameterValues = container.ParameterValues };
                        //data.PublishEvent(EventTopicNames.ExecuteEvent, true);
                        var clonedAction = ObjectCloner.Clone(action);
                        var containerParameterValues = container.ParameterValues ?? "";
                        clonedAction.Parameter = settingReplacer.ReplaceSettingValue("\\{:[^}]+\\}", clonedAction.Parameter);
                        containerParameterValues = settingReplacer.ReplaceSettingValue("\\{:[^}]+\\}", containerParameterValues);
                        var data = new ActionData { Action = clonedAction, DataObject = dataObject, ParameterValues = containerParameterValues };
                        data.PublishEvent(EventTopicNames.ExecuteEvent, true);
                    }
                }
            }
        }

        public void RegisterActionType(string actionType, string actionName, object parameterObject)
        {
            _ruleActionTypeRegistry.RegisterActionType(actionType, actionName, parameterObject);
        }

        public void RegisterEvent(string eventKey, string eventName, object constraintObject)
        {
            _ruleActionTypeRegistry.RegisterEvent(eventKey, eventName, constraintObject);
        }

        public void RegisterParameterSoruce(string parameterName, Func<IEnumerable<string>> action)
        {
            ParameterSources.Add(parameterName, action);
        }

        public IEnumerable<IRuleConstraint> GetEventConstraints(string eventName)
        {
            return _ruleActionTypeRegistry.GetEventConstraints(eventName);
        }

        public IEnumerable<RuleEvent> GetRuleEvents()
        {
            return _ruleActionTypeRegistry.RuleEvents.Values;
        }

        public IEnumerable<string> GetParameterSource(string parameterName)
        {
            return ParameterSources.GetParameterSource(parameterName);
        }

        IEnumerable<string> IAutomationService.GetParameterNames(string eventName)
        {
            return _ruleActionTypeRegistry.GetParameterNames(eventName);
        }

        public RuleActionType GetActionType(string value)
        {
            return _ruleActionTypeRegistry.ActionTypes[value];
        }

        public IEnumerable<RuleActionType> GetActionTypes()
        {
            return _ruleActionTypeRegistry.ActionTypes.Values;
        }

        public IEnumerable<IRuleConstraint> CreateRuleConstraints(string eventConstraints)
        {
            return eventConstraints.Split('#')
                .Where(x => !x.StartsWith("SN$"))
                .Select(x => new RuleConstraint(x));
        }

        public IEnumerable<IParameterValue> CreateParameterValues(RuleActionType actionType)
        {
            if (actionType.ParameterObject != null)
                return actionType.ParameterObject.GetType().GetProperties().Select(x => new ParameterValue(x));
            return new List<IParameterValue>();
        }

        public AppAction GetActionById(int appActionId)
        {
            return Dao.Single<AppAction>(x => x.Id == appActionId);
        }

        private bool SatisfiesConditions(AppRule appRule, object dataObject)
        {
            var conditions = appRule.EventConstraints.Split('#')
                .Select(x => new RuleConstraint(x));

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
                        while (Regex.IsMatch(settingName, "\\[[^\\]]+\\]"))
                        {
                            var paramvalue = Regex.Match(settingName, "\\[[^\\]]+\\]").Groups[0].Value;
                            var insideValue = paramvalue.Trim(new[] { '[', ']' });
                            if (parameterNames.Contains(insideValue))
                            {
                                var v = dataObject.GetType().GetProperty(insideValue).GetValue(dataObject, null).ToString();
                                settingName = settingName.Replace(paramvalue, v);
                            }
                            else
                            {
                                if (paramvalue == "[Day]")
                                    settingName = settingName.Replace(paramvalue, DateTime.Now.Day.ToString());
                                else if (paramvalue == "[Month]")
                                    settingName = settingName.Replace(paramvalue, DateTime.Now.Month.ToString());
                                else if (paramvalue == "[Year]")
                                    settingName = settingName.Replace(paramvalue, DateTime.Now.Year.ToString());
                                else settingName = settingName.Replace(paramvalue, "");
                            }
                        }

                        var customSettingValue = _settingService.ReadSetting(settingName).StringValue ?? "";
                        if (!condition.ValueEquals(customSettingValue)) return false;
                    }
                    if (condition.Name == "TerminalName" && !string.IsNullOrEmpty(condition.Value))
                    {
                        if (!condition.Value.Equals(_applicationState.CurrentTerminal.Name))
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
