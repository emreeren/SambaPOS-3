using System;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Infrastructure;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export]
    internal class RuleExecutor
    {
        private readonly ICacheService _cacheService;
        private readonly ConditionChecker _conditionChecker;
        private readonly ActionDataBuilder _actionDataBuilder;
        private string _eventName;
        private int _terminalId;
        private int _departmentId;
        private int _userRoleId;
        private int _ticketTypeId;

        [ImportingConstructor]
        public RuleExecutor(ICacheService cacheService, ConditionChecker conditionChecker, ActionDataBuilder actionDataBuilder)
        {
            _cacheService = cacheService;
            _conditionChecker = conditionChecker;
            _actionDataBuilder = actionDataBuilder;
        }

        public RuleExecutor SelectFor(string eventName)
        {
            _eventName = eventName;
            return this;
        }

        public RuleExecutor WithTerminalId(int terminalId)
        {
            _terminalId = terminalId;
            return this;
        }

        public RuleExecutor WithDepartmentId(int departmentId)
        {
            _departmentId = departmentId;
            return this;
        }

        public RuleExecutor WithUserRoleId(int userRoleId)
        {
            _userRoleId = userRoleId;
            return this;
        }

        public RuleExecutor WithTicketTypeId(int ticketTypeId)
        {
            _ticketTypeId = ticketTypeId;
            return this;
        }

        public void ExecuteWith(object dataParameter, Action<ActionData> dataAction)
        {
            var rules = _cacheService
                .GetAppRules(_eventName, _terminalId, _departmentId, _userRoleId, _ticketTypeId)
                .Where(x => _conditionChecker.Satisfies(x, dataParameter));

            var actionContainers = rules
                .SelectMany(rule => rule.Actions.OrderBy(x => x.SortOrder)
                                        .Where(
                                            x =>
                                            _conditionChecker.SatisfiesCustomConstraint(x.CustomConstraint,
                                                                                        dataParameter)))
                .ToList();

            var dataObject = dataParameter.ToDynamic();

            foreach (var actionContainer in actionContainers)
            {
                _actionDataBuilder
                    .CreateFor(actionContainer)
                    .PreprocessWith(dataObject)
                    .InvokeFor(dataAction);
            }
        }
    }
}