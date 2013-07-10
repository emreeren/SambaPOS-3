using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure;
using Samba.Infrastructure.Data.Serializer;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export(typeof(INotificationService))]
    class NotificationService : INotificationService
    {
        private readonly ICacheService _cacheService;
        private readonly ISettingService _settingService;
        private readonly IExpressionService _expressionService;

        [ImportingConstructor]
        public NotificationService(ICacheService cacheService, ISettingService settingService, IExpressionService expressionService)
        {
            _cacheService = cacheService;
            _settingService = settingService;
            _expressionService = expressionService;
        }

        public void NotifyEvent(string eventName, object dataParameter, int terminalId, int departmentId, int userRoleId, int ticketTypeId, Action<ActionData> dataAction)
        {
            var dataObject = dataParameter.ToDynamic();
            _settingService.ClearSettingCache();
            var rules = _cacheService.GetAppRules(eventName, terminalId, departmentId, userRoleId, ticketTypeId);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                if (!CanExecuteRule(rule, dataObject)) continue;
                foreach (var actionContainer in rule.Actions.OrderBy(x => x.SortOrder).Where(x => CanExecuteAction(x, dataObject)))
                {
                    var container = actionContainer;
                    var action = _cacheService.GetActions().Single(x => x.Id == container.AppActionId);
                    var clonedAction = ObjectCloner.Clone(action);
                    var containerParameterValues = container.ParameterValues ?? "";
                    _settingService.ClearSettingCache();
                    clonedAction.Parameter = _settingService.ReplaceSettingValues(clonedAction.Parameter);
                    containerParameterValues = _settingService.ReplaceSettingValues(containerParameterValues);
                    containerParameterValues = ReplaceParameterValues(containerParameterValues, dataObject);
                    clonedAction.Parameter = _expressionService.ReplaceExpressionValues(clonedAction.Parameter, dataObject);
                    containerParameterValues = _expressionService.ReplaceExpressionValues(containerParameterValues, dataObject);
                    var data = new ActionData { Action = clonedAction, DataObject = dataObject, ParameterValues = containerParameterValues };
                    dataAction.Invoke(data);
                }
            }
        }

        private string ReplaceParameterValues(string parameterValues, object dataObject)
        {
            if (!string.IsNullOrEmpty(parameterValues) && Regex.IsMatch(parameterValues, "\\[:([^\\]]+)\\]"))
            {
                foreach (var propertyName in Regex.Matches(parameterValues, "\\[:([^\\]]+)\\]").Cast<Match>().Select(match => match.Groups[1].Value).ToList())
                {
                    if (!((IDictionary<string, object>)dataObject).ContainsKey(propertyName)) continue;
                    var val = ((IDictionary<string, object>)dataObject)[propertyName];
                    parameterValues = parameterValues.Replace(string.Format("[:{0}]", propertyName), val != null ? val.ToString() : "");
                }
            }
            return parameterValues;
        }

        private bool CanExecuteRule(AppRule appRule, object dataObject)
        {
            if (string.IsNullOrEmpty(appRule.CustomConstraint)) return true;
            var expression = _settingService.ReplaceSettingValues(appRule.CustomConstraint);
            expression = ReplaceParameterValues(expression, dataObject);
            return _expressionService.Eval("result = " + expression, dataObject, true);
        }

        private bool CanExecuteAction(ActionContainer actionContainer, object dataObject)
        {
            if (string.IsNullOrEmpty(actionContainer.CustomConstraint)) return true;
            var expression = _settingService.ReplaceSettingValues(actionContainer.CustomConstraint);
            expression = ReplaceParameterValues(expression, dataObject);
            return _expressionService.Eval("result = " + expression, dataObject, true);
        }

        private bool SatisfiesConditions(AppRule appRule, object dataObject)
        {
            var conditions = appRule.EventConstraints.Split('#').Select(x => new RuleConstraint(x));

            var parameterNames = ((IDictionary<string, object>)dataObject).Keys;

            foreach (var condition in conditions)
            {
                var parameterName = parameterNames.FirstOrDefault(condition.Name.Equals);

                if (!string.IsNullOrEmpty(parameterName))
                {
                    var parameterValue = ((IDictionary<string, object>)dataObject)[parameterName];
                    if (condition.IsValueDifferent(parameterValue)) return false;
                }
            }

            return true;
        }
    }
}
