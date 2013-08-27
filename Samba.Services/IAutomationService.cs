using System;
using System.Collections.Generic;
using Samba.Domain.Models.Automation;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IAutomationService
    {
        IEnumerable<RuleConstraint> GetEventConstraints(string eventName);
        IEnumerable<RuleConstraint> CreateRuleConstraints(string eventConstraints);
        IEnumerable<RuleEvent> GetRuleEvents();
        IEnumerable<string> GetParameterNames(string eventName);
        IActionType GetActionType(string value);
        IEnumerable<IActionType> GetActionTypes();
        IEnumerable<ParameterValue> CreateParameterValues(IActionType actionType);
        void ProcessAction(string actionType, ActionData value);
        void RegisterParameterSource(string reportname, Func<IEnumerable<string>> func);
        void Register();
        IDictionary<string, Type> GetCustomRuleConstraintNames(string eventName);
    }
}
