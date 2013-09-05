using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export]
    internal class ConditionChecker
    {
        private readonly Evaluator _evaluator;
        private readonly Preprocessor _preprocessor;
        private object _dataObject;

        [ImportingConstructor]
        public ConditionChecker(Evaluator evaluator, Preprocessor preprocessor)
        {
            _evaluator = evaluator;
            _preprocessor = preprocessor;
        }

        public IEnumerable<string> ParameterNames { get { return DataObject.Keys; } }
        public IDictionary<string, object> DataObject { get { return ((IDictionary<string, object>)_dataObject); } }

        public bool Satisfies(AppRule appRule, object dataParameter)
        {
            _dataObject = dataParameter.ToDynamic();

            if (!SatisfiesCustomConstraints(appRule, _dataObject)) return false;

            if (!string.IsNullOrEmpty(appRule.EventConstraints) && GetConditions(appRule).Any(DoesNotSatisfy))
            {
                return false;
            }

            return SatisfiesCustomConstraint(appRule.CustomConstraint, dataParameter);
        }

        private bool SatisfiesCustomConstraints(AppRule appRule, object dataObject)
        {
            var cv = appRule.RuleConstraints;
            if (string.IsNullOrEmpty(cv)) return true;
            cv = _preprocessor.Process(cv, dataObject);
            var cvs = JsonHelper.Deserialize<List<RuleConstraintValue>>(cv);
            if (!cvs.Any()) return true;
            switch ((RuleConstraintMatch)appRule.ConstraintMatch)
            {
                case RuleConstraintMatch.MatchesAny: return cvs.Any(x => x.Satisfies(dataObject));
                case RuleConstraintMatch.MatchesAll: return cvs.All(x => x.Satisfies(dataObject));
                case RuleConstraintMatch.NotMatchesAny: return cvs.Any(x => !x.Satisfies(dataObject));
                case RuleConstraintMatch.NotMatchesAll: return cvs.All(x => !x.Satisfies(dataObject));
                default: return cvs.GroupBy(x => x.Left).All(x => x.Any(y => y.Satisfies(dataObject)));
            }
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
            if (string.IsNullOrEmpty(customConstraint)) return true;
            return _evaluator.Evals(customConstraint, dataParameter.ToDynamic(), true);
        }
    }
}