using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Automation
{
    public class AppRule : EntityClass, IOrderable
    {
        public string EventName { get; set; }
        public string EventConstraints { get; set; }
        public string CustomConstraint { get; set; }
        public string RuleConstraints { get; set; }
        public int ConstraintMatch { get; set; }

        private IList<RuleConstraintValue> _ruleConstraintValues;
        private IList<RuleConstraintValue> RuleConstraintValues
        {
            get { return _ruleConstraintValues ?? (_ruleConstraintValues = JsonHelper.Deserialize<List<RuleConstraintValue>>(RuleConstraints)); }
        }

        private IList<ActionContainer> _actions;
        public virtual IList<ActionContainer> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }

        private IList<AppRuleMap> _appRuleMaps;
        public virtual IList<AppRuleMap> AppRuleMaps
        {
            get { return _appRuleMaps; }
            set { _appRuleMaps = value; }
        }

        public AppRule()
        {
            _actions = new List<ActionContainer>();
            _appRuleMaps = new List<AppRuleMap>();
        }

        public int SortOrder { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public void AddRuleMap()
        {
            AppRuleMaps.Add(new AppRuleMap());
        }

        public RuleConstraintValue AddRuleConstraint(string left, string operation, string right)
        {
            var result = new RuleConstraintValue { Name = Utility.RandomString(10), Left = left, Operation = operation, Right = right };
            RuleConstraintValues.Add(result);
            UpdateRuleConstraints();
            return result;
        }

        public void UpdateRuleConstraint(string name, string left, string operation, string right)
        {
            if (RuleConstraintValues.All(x => x.Name != name))
            {
                RuleConstraintValues.Add(new RuleConstraintValue { Name = name });
            }
            var rc = RuleConstraintValues.First(x => x.Name == name);
            rc.Left = left;
            rc.Operation = operation;
            rc.Right = right;
            UpdateRuleConstraints();
        }

        public void DeleteRuleConstraint(string name)
        {
            RuleConstraintValues.Where(x => x.Name == name).ToList().ForEach(x => RuleConstraintValues.Remove(x));
            UpdateRuleConstraints();
        }

        public IEnumerable<RuleConstraintValue> GetRuleConstraintValues()
        {
            return RuleConstraintValues;
        }

        public void UpdateRuleConstraints()
        {
            RuleConstraints = JsonHelper.Serialize(RuleConstraintValues);
            _ruleConstraintValues = null;
        }

        public void RemoveInvalidConstraints()
        {
            RuleConstraintValues.Where(x => string.IsNullOrEmpty(x.Left) || string.IsNullOrEmpty(x.Operation)).ToList().ForEach(x => RuleConstraintValues.Remove(x));
            UpdateRuleConstraints();
        }
    }
}
