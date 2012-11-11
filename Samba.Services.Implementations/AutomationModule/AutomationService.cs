using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ComLib.Lang;
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
            _interpreter.SetFunctionCallback("Call", CallFunction);
            _interpreter.SetFunctionCallback("F", FormatFunction);
            _interpreter.SetFunctionCallback("TN", ToNumberFunction);

            _interpreter.LexReplace("Ticket", "TicketAccessor");
            _interpreter.LexReplace("Order", "OrderAccessor");
            _interpreter.LexReplace("Locator", "LocatorAccessor");
            _interpreter.Context.Plugins.RegisterAll();
            _interpreter.Context.Types.Register(typeof(LocatorAccessor), null);
            _interpreter.Context.Types.Register(typeof(TicketAccessor), null);
            _interpreter.Context.Types.Register(typeof(OrderAccessor), null);
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
            var rules = GetAppRules(eventName);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                foreach (var actionContainer in rule.Actions.Where(x => CanExecute(x, dataObject)))
                {
                    var container = actionContainer;
                    var action = Actions.SingleOrDefault(x => x.Id == container.AppActionId);

                    if (action != null)
                    {
                        var clonedAction = ObjectCloner.Clone(action);
                        var containerParameterValues = container.ParameterValues ?? "";
                        clonedAction.Parameter = _settingService.ReplaceSettingValues(clonedAction.Parameter);
                        containerParameterValues = _settingService.ReplaceSettingValues(containerParameterValues);
                        clonedAction.Parameter = ReplaceExpressionValues(clonedAction.Parameter);
                        containerParameterValues = ReplaceExpressionValues(containerParameterValues);

                        IActionData data = new ActionData { Action = clonedAction, DataObject = dataObject, ParameterValues = containerParameterValues };
                        data.PublishEvent(EventTopicNames.ExecuteEvent, true);
                    }
                }
            }
        }

        private bool CanExecute(ActionContainer actionContainer, object dataObject)
        {
            if (string.IsNullOrEmpty(actionContainer.CustomConstraint)) return true;
            return Eval("result = " + actionContainer.CustomConstraint, dataObject, true);
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
                        while (Regex.IsMatch(settingData, "\\[:[^\\]]+\\]"))
                        {
                            var paramvalue = Regex.Match(settingData, "\\[:[^\\]]+\\]").Groups[0].Value;
                            var insideValue = paramvalue.Trim(new[] { '[', ']' }).Trim(':');
                            if (parameterNames.Contains(insideValue))
                            {
                                var v = dataObject.GetType().GetProperty(insideValue).GetValue(dataObject, null).ToString();
                                settingData = settingData.Replace(paramvalue, v);
                            }
                        }

                        var customSettingValue = _settingService.ReadSetting(settingData).StringValue ?? "";
                        if (!condition.ValueEquals(customSettingValue)) return false;
                    }
                }
            }

            return true;
        }

        private static object ToNumberFunction(FunctionCallExpr arg)
        {
            double d;
            double.TryParse(arg.ParamList[0].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out d);
            return d;
        }

        private static object FormatFunction(FunctionCallExpr arg)
        {
            var fmt = arg.ParamList.Count > 1
                          ? arg.ParamList[1].ToString()
                          : "#,#0.00";
            return ((double)arg.ParamList[0]).ToString(fmt);
        }

        private object CallFunction(FunctionCallExpr arg)
        {
            return EvalCommand(arg.ParamList[0].ToString(), null, null, default(object));
        }

        public string Eval(string expression)
        {
            try
            {
                _interpreter.Execute("result = " + expression);
                return _interpreter.Result.Success ? _interpreter.Memory.Get<string>("result") : "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public T Eval<T>(string expression, object dataObject, T defaultValue = default(T))
        {
            try
            {
                if (dataObject != null)
                {
                    TicketAccessor.Model = GetDataValue<Ticket>(dataObject);
                    OrderAccessor.Model = GetDataValue<Order>(dataObject);
                }
                _interpreter.Execute(expression);
                return _interpreter.Result.Success ? _interpreter.Memory.Get<T>("result") : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public T EvalCommand<T>(string functionName, IEntity entity, object dataObject, T defaultValue = default(T))
        {
            var entityName = entity != null ? "_" + entity.Name : "";
            var script = GetScript(functionName, entityName);
            if (string.IsNullOrEmpty(script)) return defaultValue;
            return Eval(script, dataObject, defaultValue);
        }

        public string ReplaceExpressionValues(string data, string template = "\\[=([^\\]]+)\\]")
        {
            var result = data;
            while (Regex.IsMatch(result, template, RegexOptions.Singleline))
            {
                var match = Regex.Match(result, template);
                var tag = match.Groups[0].Value;
                var expression = match.Groups[1].Value.Trim();
                if (expression.StartsWith("$") && !expression.Trim().Contains(" ") && _interpreter.Memory.Contains(expression.Trim('$')))
                {
                    result = result.Replace(tag, _interpreter.Memory.Get<string>(expression.Trim('$')));
                }
                else result = result.Replace(tag, Eval(expression));
            }
            return result;
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
            if (dataObject == null) return null;
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
