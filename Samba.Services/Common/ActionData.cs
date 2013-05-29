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
            return GetDataValue<string>(dataName);
        }

        public int GetDataValueAsInt(string dataName)
        {
            int result;
            int.TryParse(GetDataValueAsString(dataName), out result);
            return result;
        }

        public bool GetAsBoolean(string parameterName)
        {
            bool result;
            bool.TryParse(GetAsString(parameterName), out result);
            return result;
        }

        public string GetAsString(string parameterName)
        {
            return Action.GetFormattedParameter(parameterName, DataObject, ParameterValues);
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
