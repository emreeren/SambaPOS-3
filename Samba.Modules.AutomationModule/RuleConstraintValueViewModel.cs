using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Services.Common;

namespace Samba.Modules.AutomationModule
{
    public class RuleConstraintValueViewModel : ObservableObject
    {
        private readonly RuleConstraintValue _model;
        private readonly IEnumerable<RuleConstraintName> _ruleConstraintNames;
        private readonly ICaptionCommand _removeConstraintCommand;
        private IEnumerable<string> _values;
        private ObservableCollection<RuleConstraintOperation> _ruleConstraintOperations;

        public RuleConstraintValueViewModel(RuleConstraintValue ruleConstraintValue, IEnumerable<RuleConstraintName> ruleConstraintNames, ICaptionCommand removeConstraintCommand)
        {
            _model = ruleConstraintValue;
            _ruleConstraintNames = ruleConstraintNames;
            _removeConstraintCommand = removeConstraintCommand;
            Values = ParameterSources.GetParameterSource(Left);
        }

        public ObservableCollection<RuleConstraintOperation> RuleConstraintOperations
        {
            get
            {
                if (_ruleConstraintOperations == null)
                {
                    _ruleConstraintOperations = new ObservableCollection<RuleConstraintOperation>();
                    _ruleConstraintOperations.AddRange(GetRuleConstraintOperations());
                }
                return _ruleConstraintOperations;
            }
        }

        public IEnumerable<RuleConstraintName> RuleConstraintNames
        {
            get { return _ruleConstraintNames; }
        }

        public IEnumerable<string> Values
        {
            get { return _values; }
            set { _values = value; RaisePropertyChanged(() => Values); }
        }

        public string Left
        {
            get { return _model.Left; }
            set
            {
                _model.Left = value;
                Values = ParameterSources.GetParameterSource(Left);
                var operationName = Operation;
                RuleConstraintOperations.Clear();
                RuleConstraintOperations.AddRange(GetRuleConstraintOperations());
                var op = RuleConstraintOperations.FirstOrDefault(x => x.Value == operationName);
                if (op != null) Operation = op.Value;
            }
        }

        public string TypedText
        {
            get { return RuleConstraintNames.Any(x => x.Name == Left) ? RuleConstraintNames.First(x => x.Name == Left).Display : Left; }
            set
            {
                if (RuleConstraintNames.All(x => x.Display != value))
                    Left = value;
            }
        }

        public string Operation { get { return _model.Operation; } set { _model.Operation = value; RaisePropertyChanged(() => Operation); } }
        public string Right { get { return _model.Right; } set { _model.Right = value; } }
        public string Name { get { return _model.Name; } }
        public ICaptionCommand RemoveConstraintCommand { get { return _removeConstraintCommand; } }

        private IEnumerable<RuleConstraintOperation> GetRuleConstraintOperations()
        {
            var entry = RuleConstraintNames.FirstOrDefault(x => x.Name == Left);
            if (entry == null)
                return Operations.AllOperations.Select(x => new RuleConstraintOperation(x));
            if (!Utility.IsNumericType(entry.Type))
                return Operations.StringOperations.Select(x => new RuleConstraintOperation(x));
            return Operations.NumericOperations.Select(x => new RuleConstraintOperation(x));
        }
    }
}