using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Data.Serializer;
using Samba.Persistance.DaoClasses;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export(typeof(IAutomationServiceBase))]
    class AutomationServiceBase : IAutomationServiceBase
    {
        private readonly ISettingService _settingService;
        private readonly IExpressionService _expressionService;
        private readonly IAutomationDao _automationDao;
        private readonly RuleActionTypeRegistry _ruleActionTypeRegistry;

        [ImportingConstructor]
        public AutomationServiceBase(IAutomationDao automationDao, ISettingService settingService, IExpressionService expressionService)
        {
            _settingService = settingService;
            _expressionService = expressionService;
            _automationDao = automationDao;
            _ruleActionTypeRegistry = new RuleActionTypeRegistry();
        }

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = _automationDao.GetRules()); } }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = _automationDao.GetActions()); } }

        public void NotifyEvent(string eventName, object dataObject, int terminalId, int departmentId, int userRoleId, Action<IActionData> dataAction)
        {
            var rules = GetAppRules(eventName, terminalId, departmentId, userRoleId);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                if (!CanExecuteRule(rule, dataObject)) continue;
                foreach (var actionContainer in rule.Actions.Where(x => CanExecuteAction(x, dataObject)))
                {
                    var container = actionContainer;
                    var action = Actions.Single(x => x.Id == container.AppActionId);
                    var clonedAction = ObjectCloner.Clone(action);
                    var containerParameterValues = container.ParameterValues ?? "";
                    _settingService.ClearSettingCache();
                    clonedAction.Parameter = _settingService.ReplaceSettingValues(clonedAction.Parameter);
                    containerParameterValues = _settingService.ReplaceSettingValues(containerParameterValues);
                    containerParameterValues = ReplaceParameterValues(containerParameterValues, dataObject);
                    clonedAction.Parameter = _expressionService.ReplaceExpressionValues(clonedAction.Parameter, dataObject);
                    containerParameterValues = _expressionService.ReplaceExpressionValues(containerParameterValues, dataObject);
                    IActionData data = new ActionData { Action = clonedAction, DataObject = dataObject, ParameterValues = containerParameterValues };
                    dataAction.Invoke(data);
                }
            }
        }

        public IEnumerable<AppRule> GetAppRules(string eventName, int terminalId, int departmentId, int userRoleId)
        {
            var maps = Rules.Where(x => x.EventName == eventName).SelectMany(x => x.AppRuleMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return Rules.Where(x => maps.Any(y => y.AppRuleId == x.Id)).OrderBy(x => x.SortOrder);
        }

        private string ReplaceParameterValues(string parameterValues, object dataObject)
        {
            if (!string.IsNullOrEmpty(parameterValues) && Regex.IsMatch(parameterValues, "\\[:([^\\]]+)\\]"))
            {
                foreach (var propertyName in Regex.Matches(parameterValues, "\\[:([^\\]]+)\\]").Cast<Match>().Select(match => match.Groups[1].Value).ToList())
                {
                    var prop = dataObject.GetType().GetProperty(propertyName);
                    if (prop == null) continue;
                    var val = prop.GetValue(dataObject, null);
                    parameterValues = parameterValues.Replace(string.Format("[:{0}]", propertyName), val != null ? val.ToString() : "");
                }
            }
            return parameterValues;
        }

        private bool CanExecuteRule(AppRule appRule, object dataObject)
        {
            if (string.IsNullOrEmpty(appRule.CustomConstraint)) return true;
            _settingService.ReplaceSettingValues(appRule.CustomConstraint);
            return _expressionService.Eval("result = " + appRule.CustomConstraint, dataObject, true);
        }

        private bool CanExecuteAction(ActionContainer actionContainer, object dataObject)
        {
            if (string.IsNullOrEmpty(actionContainer.CustomConstraint)) return true;
            _settingService.ReplaceSettingValues(actionContainer.CustomConstraint);
            return _expressionService.Eval("result = " + actionContainer.CustomConstraint, dataObject, true);
        }

        private bool SatisfiesConditions(AppRule appRule, object dataObject)
        {
            var conditions = appRule.EventConstraints.Split('#').Select(x => new RuleConstraint(x));

            var parameterNames = dataObject.GetType().GetProperties().Select(x => x.Name).ToList();

            foreach (var condition in conditions)
            {
                var parameterName = parameterNames.FirstOrDefault(condition.Name.Equals);

                if (!string.IsNullOrEmpty(parameterName))
                {
                    var property = dataObject.GetType().GetProperty(parameterName);
                    var parameterValue = property.GetValue(dataObject, null) ?? "";
                    if (condition.IsValueDifferent(parameterValue)) return false;
                }
            }

            return true;
        }

        public IEnumerable<IRuleConstraint> CreateRuleConstraints(string eventConstraints)
        {
            return eventConstraints.Split('#')
                .Select(x => new RuleConstraint(x));
        }

        public void RegisterActionType(string actionType, string actionName, object parameterObject)
        {
            _ruleActionTypeRegistry.RegisterActionType(actionType, actionName, parameterObject);
        }

        public void RegisterEvent(string eventKey, string eventName, object constraintObject)
        {
            _ruleActionTypeRegistry.RegisterEvent(eventKey, eventName, constraintObject);
        }

        public IEnumerable<IRuleConstraint> GetEventConstraints(string eventName)
        {
            return _ruleActionTypeRegistry.GetEventConstraints(eventName);
        }

        public IEnumerable<RuleEvent> GetRuleEvents()
        {
            return _ruleActionTypeRegistry.RuleEvents.Values;
        }

        IEnumerable<string> IAutomationServiceBase.GetParameterNames(string eventName)
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

        public IEnumerable<IParameterValue> CreateParameterValues(RuleActionType actionType)
        {
            if (actionType.ParameterObject != null)
                return actionType.ParameterObject.GetType().GetProperties().Select(x => new ParameterValue(x));
            return new List<IParameterValue>();
        }

        public AppAction GetActionById(int appActionId)
        {
            return _automationDao.GetActionById(appActionId);
        }

        public IEnumerable<string> GetAutomationCommandNames()
        {
            return _automationDao.GetAutomationCommandNames();
        }

        public void RegisterParameterSoruce(string parameterName, Func<IEnumerable<string>> action)
        {
            ParameterSources.Add(parameterName, action);
        }

    }
}
