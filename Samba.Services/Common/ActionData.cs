using System.Collections.Generic;
using Samba.Domain.Models.Automation;

namespace Samba.Services.Common
{
    public class ActionData
    {
        public AppAction Action { get; set; }
        public string ParameterValues { get; set; }
        public dynamic DataObject { get; set; }

        public T GetDataValue<T>(string dataName) where T : class
        {
            if (!((IDictionary<string, object>)DataObject).ContainsKey(dataName)) return null;
            return ((IDictionary<string, object>)DataObject)[dataName] as T;
        }

        public string GetDataValueAsString(string dataName)
        {
            if (!((IDictionary<string, object>)DataObject).ContainsKey(dataName)) return "";
            return ((IDictionary<string, object>)DataObject)[dataName].ToString();
        }

        public void SetDataValue(string propertyName, string value)
        {
            if (!((IDictionary<string, object>)DataObject).ContainsKey(propertyName))
                ((IDictionary<string, object>)DataObject).Add(propertyName, value);
            else
                ((IDictionary<string, object>)DataObject)[propertyName] = value;
        }

        public int GetDataValueAsInt(string dataName)
        {
            int result;
            int.TryParse(GetDataValueAsString(dataName), out result);
            return result;
        }

        public bool GetAsBoolean(string parameterName, bool defaultValue = false)
        {
            bool result;
            return bool.TryParse(GetAsString(parameterName), out result) ? result : defaultValue;
        }

        public string GetAsString(string parameterName)
        {
            var result = Action.GetFormattedParameter(parameterName, DataObject, ParameterValues);
            return !string.IsNullOrEmpty(result) ? result : "";
        }

        public decimal GetAsDecimal(string parameterName)
        {
            decimal result;
            decimal.TryParse(GetAsString(parameterName), out result);
            return result;
        }

        public int GetAsInteger(string parameterName)
        {
            int result;
            int.TryParse(GetAsString(parameterName), out result);
            return result;
        }
    }
}
