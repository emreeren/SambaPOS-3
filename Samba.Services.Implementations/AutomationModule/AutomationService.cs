using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using ComLib.Lang;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Persistance.Data;
using Samba.Services.Common;
using Samba.Services.Implementations.AutomationModule.Accessors;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export(typeof(IAutomationService))]
    class AutomationService : AbstractService, IAutomationService
    {
        private readonly IApplicationState _applicationState;
        private readonly RuleActionTypeRegistry _ruleActionTypeRegistry;
        private readonly ISettingService _settingService;

        private readonly Interpreter _interpreter;
        private Dictionary<string, string> _scripts;

        [ImportingConstructor]
        public AutomationService(IApplicationState applicationState, ISettingService settingService)
        {
            _applicationState = applicationState;
            _ruleActionTypeRegistry = new RuleActionTypeRegistry();
            _settingService = settingService;

            _scripts = Dao.Query<Script>().ToDictionary(x => x.HandlerName, x => x.Code);
            _interpreter = new Interpreter();
            _interpreter.LexReplace("Ticket", "TicketAccessor");
            _interpreter.Context.Plugins.RegisterAll();
            _interpreter.Context.Types.Register(typeof(IServiceLocator), null);
            _interpreter.Memory.SetValue("Locator", ServiceLocator.Current);
            _interpreter.Context.Types.Register(typeof(TicketAccessor), null);
        }

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = Dao.Query<AppRule>(x => x.Actions, x => x.AppRuleMaps).OrderBy(x => x.Order)); } }


        public IEnumerable<AppRule> GetAppRules(string eventName)
        {
            var maps = Rules.Where(x => x.EventName == eventName).SelectMany(x => x.AppRuleMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return Rules.Where(x => maps.Any(y => y.AppRuleId == x.Id)).OrderBy(x => x.Order);
        }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = Dao.Query<AppAction>().OrderBy(x => x.Order)); } }

        public void NotifyEvent(string eventName, object dataObject)
        {
            var settingReplacer = _settingService.GetSettingReplacer();
            var rules = GetAppRules(eventName);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                foreach (var actionContainer in rule.Actions)
                {
                    var container = actionContainer;
                    var action = Actions.SingleOrDefault(x => x.Id == container.AppActionId);

                    if (action != null)
                    {
                        var clonedAction = ObjectCloner.Clone(action);
                        var containerParameterValues = container.ParameterValues ?? "";
                        clonedAction.Parameter = settingReplacer.ReplaceSettingValue("\\{:([^}]+)\\}", clonedAction.Parameter);
                        containerParameterValues = settingReplacer.ReplaceSettingValue("\\{:([^}]+)\\}", containerParameterValues);
                        IActionData data = new ActionData { Action = clonedAction, DataObject = dataObject, ParameterValues = containerParameterValues };
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

        public IEnumerable<string> GetAutomationCommandNames()
        {
            return Dao.Distinct<AutomationCommand>(x => x.Name);
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
                else
                {
                    if (condition.Name.StartsWith("SN$"))
                    {
                        var settingData = condition.Name.Replace("SN$", "");
                        while (Regex.IsMatch(settingData, "\\[[^\\]]+\\]"))
                        {
                            var paramvalue = Regex.Match(settingData, "\\[[^\\]]+\\]").Groups[0].Value;
                            var insideValue = paramvalue.Trim(new[] { '[', ']' });
                            if (parameterNames.Contains(insideValue))
                            {
                                var v = dataObject.GetType().GetProperty(insideValue).GetValue(dataObject, null).ToString();
                                settingData = settingData.Replace(paramvalue, v);
                            }
                            else
                            {
                                if (paramvalue == "[Day]")
                                    settingData = settingData.Replace(paramvalue, DateTime.Now.Day.ToString());
                                else if (paramvalue == "[Month]")
                                    settingData = settingData.Replace(paramvalue, DateTime.Now.Month.ToString());
                                else if (paramvalue == "[Year]")
                                    settingData = settingData.Replace(paramvalue, DateTime.Now.Year.ToString());
                                else settingData = settingData.Replace(paramvalue, "");
                            }
                        }

                        var customSettingValue = _settingService.ReadSetting(settingData).StringValue ?? "";
                        if (!condition.ValueEquals(customSettingValue)) return false;
                    }
                }
            }

            return true;
        }

        public string Eval(string expression)
        {
            _interpreter.Execute("result = " + expression);
            return _interpreter.Memory.Get<string>("result");
        }

        public T EvalCommand<T>(string functionName, IEntity entity, object dataObject, T defaultValue = default(T))
        {
            var entityName = entity != null ? "_" + entity.Name : "";
            var script = GetScript(functionName, entityName);
            if (string.IsNullOrEmpty(script)) return defaultValue;
            try
            {
                TicketAccessor.Model = GetDataValue<Ticket>(dataObject);
                _interpreter.Execute(script);
                return _interpreter.Memory.Get<T>("result");
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private string GetScript(string functionName, string entityName)
        {
            if (_scripts.ContainsKey(functionName + entityName))
                return _scripts[functionName + entityName];
            if (_scripts.ContainsKey(functionName + "_*"))
                return _scripts[functionName + "_*"];
            return "";
        }

        private static T GetDataValue<T>(object dataObject) where T : class
        {
            var property = dataObject.GetType().GetProperty(typeof(T).Name);
            if (property != null)
                return property.GetValue(dataObject, null) as T;
            return null;
        }

        public override void Reset()
        {
            _scripts.Clear();
            _scripts = Dao.Query<Script>().ToDictionary(x => x.HandlerName, x => x.Code);
            _rules = null;
            _actions = null;
        }
    }
}
