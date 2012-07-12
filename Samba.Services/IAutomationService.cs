using System;
using System.Collections.Generic;
using Samba.Domain.Models.Actions;

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

    public class AutomationCommandData
    {
        public AutomationCommand AutomationCommand { get; set; }
        public bool DisplayOnTicket { get; set; }
        public bool DisplayOnPayment { get; set; }
        public int VisualBehaviour { get; set; }
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
        IEnumerable<IRuleConstraint> CreateRuleConstraints(string eventConstraints);
        IEnumerable<IParameterValue> CreateParameterValues(RuleActionType actionType);
        AppAction GetActionById(int appActionId);
    }
}
