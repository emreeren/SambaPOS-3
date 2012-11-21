using System;
using System.Collections.Generic;
using Samba.Domain.Models.Automation;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
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
        IEnumerable<string> GetAutomationCommandNames();
    }
}
