using System;
using System.Collections.Generic;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IAutomationService
    {
        IEnumerable<RuleConstraint> GetEventConstraints(string eventName);
        IEnumerable<RuleConstraint> CreateRuleConstraints(string eventConstraints);
        IEnumerable<RuleEvent> GetRuleEvents();
        IEnumerable<string> GetParameterNames(string eventName);
        RuleActionType GetActionType(string value);
        IEnumerable<RuleActionType> GetActionTypes();
        IEnumerable<ParameterValue> CreateParameterValues(RuleActionType actionType);
        void ProcessAction(string actionType, ActionData value);
        void RegisterParameterSource(string reportname, Func<IEnumerable<string>> func);
        void Register();
    }
}
