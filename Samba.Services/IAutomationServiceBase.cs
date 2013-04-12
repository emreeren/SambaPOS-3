using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Automation;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IAutomationServiceBase
    {
        void NotifyEvent(string eventName, object dataObject, int terminalId, int departmentId, int userRoleId, Action<IActionData> dataAction);
        IEnumerable<IRuleConstraint> GetEventConstraints(string eventName);
        IEnumerable<IRuleConstraint> CreateRuleConstraints(string eventConstraints);
        IEnumerable<RuleEvent> GetRuleEvents();
        IEnumerable<string> GetParameterNames(string eventName);
        RuleActionType GetActionType(string value);
        IEnumerable<RuleActionType> GetActionTypes();
        IEnumerable<IParameterValue> CreateParameterValues(RuleActionType actionType);
        AppAction GetActionById(int appActionId);
        IEnumerable<string> GetAutomationCommandNames();
        void RegisterActionType(string actionType, string actionName, object parameterObject = null);
        void RegisterEvent(string eventKey, string eventName, object constraintObject = null);
        void RegisterParameterSoruce(string username, Func<IEnumerable<string>> func);
    }
}
