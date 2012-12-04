using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Data.Serializer;
using Samba.Persistance.DaoClasses;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.AutomationModule
{
    [Export(typeof(IAutomationService))]
    class AutomationService : AbstractService, IAutomationService
    {
        private readonly IAutomationDao _automationDao;
        private readonly IApplicationState _applicationState;
        private readonly RuleActionTypeRegistry _ruleActionTypeRegistry;
        private readonly ISettingService _settingService;
        private readonly IExpressionService _expressionService;

        [ImportingConstructor]
        public AutomationService(IAutomationDao automationDao, IApplicationState applicationState, ISettingService settingService, IExpressionService expressionService)
        {
            _automationDao = automationDao;
            _applicationState = applicationState;
            _ruleActionTypeRegistry = new RuleActionTypeRegistry();
            _settingService = settingService;
            _expressionService = expressionService;
        }

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = _automationDao.GetRules()); } }

        public IEnumerable<AppRule> GetAppRules(string eventName)
        {
            var maps = Rules.Where(x => x.EventName == eventName).SelectMany(x => x.AppRuleMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return Rules.Where(x => maps.Any(y => y.AppRuleId == x.Id)).OrderBy(x => x.SortOrder);
        }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = _automationDao.GetActions()); } }

        public void NotifyEvent(string eventName, object dataObject)
        {
            var rules = GetAppRules(eventName);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                if (!CanExecuteRule(rule, dataObject)) continue;
                foreach (var actionContainer in rule.Actions.Where(x => CanExecuteAction(x, dataObject)))
                {
                    var container = actionContainer;
                    var action = Actions.SingleOrDefault(x => x.Id == container.AppActionId);

                    if (action != null)
                    {
                        var clonedAction = ObjectCloner.Clone(action);
                        var containerParameterValues = container.ParameterValues ?? "";
                        clonedAction.Parameter = _settingService.ReplaceSettingValues(clonedAction.Parameter);
                        containerParameterValues = _settingService.ReplaceSettingValues(containerParameterValues);
                        clonedAction.Parameter = _expressionService.ReplaceExpressionValues(clonedAction.Parameter);
                        containerParameterValues = _expressionService.ReplaceExpressionValues(containerParameterValues);

                        IActionData data = new ActionData { Action = clonedAction, DataObject = dataObject, ParameterValues = containerParameterValues };
                        data.PublishEvent(EventTopicNames.ExecuteEvent, true);
                    }
                }
            }
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
            return _automationDao.GetActionById(appActionId);
        }

        public IEnumerable<string> GetAutomationCommandNames()
        {
            return _automationDao.GetAutomationCommandNames();
        }

        private bool SatisfiesConditions(AppRule appRule, object dataObject)
        {
            var conditions = appRule.EventConstraints.Split('#')
                .Select(x => new RuleConstraint(x));

            var parameterNames = dataObject.GetType().GetProperties().Select(x => x.Name).ToList();

            foreach (var condition in conditions)
            {
                var parameterName = parameterNames.FirstOrDefault(condition.Name.Equals);

                if (!string.IsNullOrEmpty(parameterName))
                {
                    var property = dataObject.GetType().GetProperty(parameterName);
                    var parameterValue = property.GetValue(dataObject, null) ?? "";
                    if (!condition.ValueEquals(parameterValue)) return false;
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
