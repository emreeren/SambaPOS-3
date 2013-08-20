using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export]
    internal class ConditionChecker
    {
        private readonly Evaluator _evaluator;
        private object _dataObject;

        [ImportingConstructor]
        public ConditionChecker(Evaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public IEnumerable<string> ParameterNames { get { return DataObject.Keys; } }
        public IDictionary<string, object> DataObject { get { return ((IDictionary<string, object>)_dataObject); } }

        public bool Satisfies(AppRule appRule, object dataParameter)
        {
            if (string.IsNullOrEmpty(appRule.EventConstraints)) return true;

            _dataObject = dataParameter.ToDynamic();

            if (GetConditions(appRule).Any(DoesNotSatisfy))
            {
                return false;
            }

            return SatisfiesCustomConstraint(appRule.CustomConstraint, dataParameter);
        }

        private static IEnumerable<RuleConstraint> GetConditions(AppRule appRule)
        {
            return appRule.EventConstraints.Split('#').Select(x => new RuleConstraint(x));
        }

        public bool DoesNotSatisfy(RuleConstraint condition)
        {
            if (!ParameterNames.Any(x => condition.Name.Equals(x))) return false;
            var parameterName = ParameterNames.First(condition.Name.Equals);
            return condition.IsValueDifferent(DataObject[parameterName]);
        }

        public bool SatisfiesCustomConstraint(string customConstraint, object dataParameter)
        {
            return _evaluator.Evals(customConstraint, dataParameter.ToDynamic(), true);
        }
    }
}