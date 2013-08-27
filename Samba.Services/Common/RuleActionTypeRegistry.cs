using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Infrastructure;
using Samba.Infrastructure.Helpers;
using Samba.Services.Implementations.AutomationModule;

namespace Samba.Services.Common
{
    [Export]
    public class RuleActionTypeRegistry
    {
        public IDictionary<string, RuleEvent> RuleEvents = new Dictionary<string, RuleEvent>();

        public IEnumerable<string> GetParameterNames(string eventKey)
        {
            var po = RuleEvents[eventKey].ParameterObject;
            return ((IDictionary<string, object>)po).Keys;
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

        [ImportMany]
        public IEnumerable<IActionType> ActionTypes { get; set; }

        public IEnumerable<RuleConstraint> GetEventConstraints(string eventName)
        {
            var result = new List<RuleConstraint>();
            var obj = RuleEvents[eventName].ParameterObject;
            if (obj != null)
            {
                var items = obj.Select(
                        x => CreateRuleConstraint(x.Key, OperatorConstants.Equal, GetOperations(x.Value.GetType())));
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
            if (Utility.IsNumericType(type))
            {
                return new[] { OperatorConstants.Equal, OperatorConstants.NotEqual, OperatorConstants.Greater, OperatorConstants.Less };
            }
            return new[] { OperatorConstants.Equal, OperatorConstants.NotEqual, OperatorConstants.Contain, OperatorConstants.NotContain, OperatorConstants.RegularExpressionMatch, OperatorConstants.NotRegularExpressionMatch };
        }

        public void ProcessAction(string actionType, ActionData actionData)
        {
            var actionProcessor = ActionTypes.FirstOrDefault(x => x.Handles(actionType));
            if (actionProcessor != null) actionProcessor.Process(actionData);
        }

        public IDictionary<string, Type> GetCustomRuleConstraintNames(string eventName)
        {
            var obj = RuleEvents[eventName].ParameterObject;
            return obj != null ? obj.ToDictionary(x => x.Key, x => x.Value.GetType()) : new Dictionary<string, Type>();
        }
    }
}
