using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Samba.Localization.Properties;

namespace Samba.Services
{
    public static class OpConst
    {
        public const string Equal = "=";
        public const string NotEqual = "!=";
        public const string Contain = "?";
        public const string NotContain = "!?";
        public const string Greater = ">";
        public const string Less = "<";
    }

    public class RuleConstraintViewModel
    {
        public RuleConstraintViewModel()
        {

        }

        public RuleConstraintViewModel(string constraintData)
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
            get { return _values ?? (_values = RuleActionTypeRegistry.GetParameterSource(Name)); }
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

                if (Operation.Contains(OpConst.NotEqual))
                {
                    if (propertyValue.Equals(objectValue)) return false;
                }
                else if (Operation.Contains(OpConst.Equal))
                {
                    if (!propertyValue.Equals(objectValue)) return false;
                }
                else if (Operation.Contains(OpConst.Greater))
                {
                    if (propertyValue < objectValue) return false;
                }
                else if (Operation.Contains(OpConst.Less))
                {
                    if (propertyValue > objectValue) return false;
                }
            }
            else
            {
                var propertyValue = parameterValue.ToString().ToLower();
                var objectValue = Value.ToLower();

                if (Operation.Contains(OpConst.NotContain))
                {
                    if (propertyValue.Contains(objectValue)) return false;
                }
                else if (Operation.Contains(OpConst.Contain))
                {
                    if (!propertyValue.Contains(objectValue)) return false;
                }
                else if (Operation.Contains(OpConst.NotEqual))
                {
                    if (propertyValue.Equals(objectValue)) return false;
                }
                else if (Operation.Contains(OpConst.Equal))
                {
                    if (!propertyValue.Equals(objectValue)) return false;
                }
            }
            return true;
        }
    }

    public class RuleActionType
    {
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public object ParameterObject { get; set; }
    }

    public class RuleEvent
    {
        public string EventKey { get; set; }
        public string EventName { get; set; }
        public object ParameterObject { get; set; }
    }

    public static class RuleActionTypeRegistry
    {
        public static IDictionary<string, RuleEvent> RuleEvents = new Dictionary<string, RuleEvent>();
        public static IDictionary<string, Func<IEnumerable<string>>> ParameterSource = new Dictionary<string, Func<IEnumerable<string>>>();

        public static IEnumerable<string> GetParameterNames(string eventKey)
        {
            var po = RuleEvents[eventKey].ParameterObject;
            return po != null ? po.GetType().GetProperties().Select(x => x.Name) : new List<string>();
        }

        public static void RegisterEvent(string eventKey, string eventName)
        {
            RegisterEvent(eventKey, eventName, null);
        }

        public static void RegisterEvent(string eventKey, string eventName, object constraintObject)
        {
            if (!RuleEvents.ContainsKey(eventKey))
                RuleEvents.Add(eventKey, new RuleEvent
                {
                    EventKey = eventKey,
                    EventName = eventName,
                    ParameterObject = constraintObject
                });
        }

        public static IDictionary<string, RuleActionType> ActionTypes = new Dictionary<string, RuleActionType>();
        public static void RegisterActionType(string actionType, string actionName, object parameterObject = null)
        {
            if (!ActionTypes.ContainsKey(actionType))
                ActionTypes.Add(actionType, new RuleActionType
                                                {
                                                    ActionName = actionName,
                                                    ActionType = actionType,
                                                    ParameterObject = parameterObject
                                                });
        }

        public static IEnumerable<RuleConstraintViewModel> GetEventConstraints(string eventName)
        {
            var result = new List<RuleConstraintViewModel>();
            var obj = RuleEvents[eventName].ParameterObject;
            if (obj != null)
            {
                result.AddRange(obj.GetType().GetProperties().Select(
                    x => new RuleConstraintViewModel { Name = x.Name, Operation = OpConst.Equal, Operations = GetOperations(x.PropertyType) }));
            }

            if (!result.Any(x => x.Name == "UserName"))
                result.Insert(0, new RuleConstraintViewModel { Name = "UserName", Operation = OpConst.Equal, Operations = GetOperations(typeof(string)) });
            if (!result.Any(x => x.Name == "DepartmentName"))
                result.Insert(0, new RuleConstraintViewModel { Name = "DepartmentName", Operation = OpConst.Equal, Operations = GetOperations(typeof(string)) });
            if (!result.Any(x => x.Name == "TerminalName"))
                result.Insert(0, new RuleConstraintViewModel { Name = "TerminalName", Operation = OpConst.Equal, Operations = GetOperations(typeof(string)) });

            return result;
        }

        private static string[] GetOperations(Type type)
        {
            if (RuleConstraintViewModel.IsNumericType(type))
            {
                return new[] { OpConst.Equal, OpConst.NotEqual, OpConst.Greater, OpConst.Less };
            }
            return new[] { OpConst.Equal, OpConst.NotEqual, OpConst.Contain, OpConst.NotContain };
        }

        public static void RegisterParameterSoruce(string parameterName, Func<IEnumerable<string>> action)
        {
            ParameterSource.Add(parameterName, action);
        }

        public static IEnumerable<string> GetParameterSource(string parameterName)
        {
            return ParameterSource.ContainsKey(parameterName) ? ParameterSource[parameterName].Invoke() : new List<string>();
        }
    }
}
