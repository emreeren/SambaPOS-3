using System;
using System.Collections.Generic;
using System.Linq;

namespace Samba.Services.Implementations.AutomationModule
{
    public class RuleActionTypeRegistry
    {
        public IDictionary<string, RuleEvent> RuleEvents = new Dictionary<string, RuleEvent>();

        public IEnumerable<string> GetParameterNames(string eventKey)
        {
            var po = RuleEvents[eventKey].ParameterObject;
            return po != null ? po.GetType().GetProperties().Select(x => x.Name) : new List<string>();
        }

        public void RegisterEvent(string eventKey, string eventName, object constraintObject)
        {
            if (!RuleEvents.ContainsKey(eventKey))
                RuleEvents.Add(eventKey, new RuleEvent
                {
                    EventKey = eventKey,
                    EventName = eventName,
                    ParameterObject = constraintObject
                });
        }

        public IDictionary<string, RuleActionType> ActionTypes = new Dictionary<string, RuleActionType>();

        public void RegisterActionType(string actionType, string actionName, object parameterObject = null)
        {
            if (!ActionTypes.ContainsKey(actionType))
                ActionTypes.Add(actionType, new RuleActionType
                                                {
                                                    ActionName = actionName,
                                                    ActionType = actionType,
                                                    ParameterObject = parameterObject
                                                });
        }

        public IEnumerable<IRuleConstraint> GetEventConstraints(string eventName)
        {
            var result = new List<IRuleConstraint>();
            var obj = RuleEvents[eventName].ParameterObject;
            if (obj != null)
            {
                var items = obj.GetType().GetProperties().Select(
                        x => CreateRuleConstraint(x.Name, OperatorConstants.Equal, GetOperations(x.PropertyType)));
                result.AddRange(items);
            }

            if (!result.Any(x => x.Name == "UserName")) result.Insert(0, CreateRuleConstraint("UserName", OperatorConstants.Equal));
            if (!result.Any(x => x.Name == "DepartmentName")) result.Insert(0, CreateRuleConstraint("DepartmentName", OperatorConstants.Equal));
            if (!result.Any(x => x.Name == "TerminalName")) result.Insert(0, CreateRuleConstraint("TerminalName", OperatorConstants.Equal));

            return result;
        }

        private static IRuleConstraint CreateRuleConstraint(string name, string operation, string[] operations = null)
        {
            if (operations == null) operations = GetOperations(typeof(string));
            return new RuleConstraint { Name = name, Operation = operation, Operations = operations };
        }

        private static string[] GetOperations(Type type)
        {
            if (RuleConstraint.IsNumericType(type))
            {
                return new[] { OperatorConstants.Equal, OperatorConstants.NotEqual, OperatorConstants.Greater, OperatorConstants.Less };
            }
            return new[] { OperatorConstants.Equal, OperatorConstants.NotEqual, OperatorConstants.Contain, OperatorConstants.NotContain };
        }
    }
}
