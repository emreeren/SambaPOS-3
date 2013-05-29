using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure;
using Samba.Services.Implementations.AutomationModule;

namespace Samba.Services.Common
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
                    ParameterObject = constraintObject.ToDynamic()
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
                                                    ParameterObject = parameterObject.ToDynamic()
                                                });
        }

        public IEnumerable<RuleConstraint> GetEventConstraints(string eventName)
        {
            var result = new List<RuleConstraint>();
            var obj = RuleEvents[eventName].ParameterObject;
            if (obj != null)
            {
                var items = obj.GetType().GetProperties().Select(
                        x => CreateRuleConstraint(x.Name, OperatorConstants.Equal, GetOperations(x.PropertyType)));
                result.AddRange(items);
            }
            return result;
        }

        private static RuleConstraint CreateRuleConstraint(string name, string operation, string[] operations = null)
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
