using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Localization.Properties;
using Samba.Services;

namespace Samba.Modules.AutomationModule.ServiceImplementations
{
    [Export(typeof(IRuleConstraint))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RuleConstraint : IRuleConstraint
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

        public static bool IsNumericType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;

        }

        public bool ValueEquals(object parameterValue)
        {
            if (IsNumericType(parameterValue.GetType()))
            {
                decimal propertyValue;
                decimal.TryParse(parameterValue.ToString(), out propertyValue);

                decimal objectValue;
                decimal.TryParse(Value, out objectValue);

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
                    if (propertyValue < objectValue) return false;
                }
                else if (Operation.Contains(OperatorConstants.Less))
                {
                    if (propertyValue > objectValue) return false;
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
            }
            return true;
        }
    }
}