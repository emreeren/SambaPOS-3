using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Services.Implementations.AutomationModule;

namespace Samba.Services.Common
{
    public class RuleConstraint
    {
        public RuleConstraint()
        {
        }

        public RuleConstraint(string constraintData)
        {
            var parts = constraintData.Split(';');
            Name = parts[0];
            Operation = parts[1];
            if (parts.Count() > 2)
                Value = parts[2];
        }

        public string Name { get; set; }
        public string NameDisplay
        {
            get
            {
                var result = Resources.ResourceManager.GetString(Name);
                return !string.IsNullOrEmpty(result) ? result + ":" : Name;
            }
        }

        public string Value { get; set; }

        private IEnumerable<string> _values;
        public IEnumerable<string> Values
        {
            get { return _values ?? (_values = ParameterSources.GetParameterSource(Name)); }
        }

        public string Operation { get; set; }
        public string[] Operations { get; set; }

        public string GetConstraintData()
        {
            return Name + ";" + Operation + ";" + Value;
        }

        public bool IsValueDifferent(object parameterValue)
        {
            return !ValueEquals(parameterValue);
        }

        public bool ValueEquals(object parameterValue)
        {
            if (parameterValue == null) return false;
            
            if (Utility.IsNumericType(parameterValue.GetType()) || Operation.Contains(OperatorConstants.Greater) || Operation.Contains(OperatorConstants.Less))
            {
                decimal propertyValue;
                decimal.TryParse(parameterValue.ToString(), out propertyValue);

                decimal objectValue;
                decimal.TryParse(Value, out objectValue);

                if (objectValue < 0 || propertyValue < 0) return false;

                if (Operation.Contains(OperatorConstants.NotEqual))
                {
                    if (propertyValue.Equals(objectValue)) return false;
                }
                else if (Operation.Contains(OperatorConstants.Equal))
                {
                    if (!propertyValue.Equals(objectValue)) return false;
                }
                else if (Operation.Contains(OperatorConstants.Greater))
                {
                    if (propertyValue <= objectValue) return false;
                }
                else if (Operation.Contains(OperatorConstants.Less))
                {
                    if (propertyValue >= objectValue) return false;
                }
            }
            else
            {
                var propertyValue = parameterValue.ToString().ToLower();
                var objectValue = Value.ToLower();

                if (Operation.Contains(OperatorConstants.NotContain))
                {
                    if (propertyValue.Contains(objectValue)) return false;
                }
                else if (Operation.Contains(OperatorConstants.Contain))
                {
                    if (!propertyValue.Contains(objectValue)) return false;
                }
                else if (Operation.Contains(OperatorConstants.NotEqual))
                {
                    if (propertyValue.Equals(objectValue)) return false;
                }
                else if (Operation.Contains(OperatorConstants.Equal))
                {
                    if (!propertyValue.Equals(objectValue)) return false;
                }
                else if (Operation.Contains(OperatorConstants.RegularExpressionMatch))
                {
                    Regex objectExpression = new Regex(objectValue);
                    if (!objectExpression.IsMatch(propertyValue)) return false;
                }
                else if (Operation.Contains(OperatorConstants.NotRegularExpressionMatch))
                {
                    Regex objectExpression = new Regex(objectValue);
                    if (objectExpression.IsMatch(propertyValue)) return false;
                }
            }
            return true;
        }
    }
}