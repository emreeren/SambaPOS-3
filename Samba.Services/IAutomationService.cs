using System;
using System.Collections.Generic;

namespace Samba.Services
{
    public class RuleActionType
    {
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public object ParameterObject { get; set; }
    }

    public class RuleEvent
    {
        public string EventKey { get; set; }
        public string EventName { get; set; }
        public object ParameterObject { get; set; }
    }

    public interface IAutomationService
    {
        void NotifyEvent(string eventName, object dataObject);
        void RegisterActionType(string actionType, string actionName, object parameterObject = null);
        void RegisterEvent(string eventKey, string eventName, object constraintObject = null);
        void RegisterParameterSoruce(string username, Func<IEnumerable<string>> func);

        IEnumerable<IRuleConstraint> GetEventConstraints(string eventName);
        IEnumerable<RuleEvent> GetRuleEvents();
        IEnumerable<string> GetParameterNames(string eventName);
        RuleActionType GetActionType(string value);
        IEnumerable<RuleActionType> GetActionTypes();
    }
}
