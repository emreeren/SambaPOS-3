using Samba.Domain.Models.Automation;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services.Implementations.AutomationModule
{
    public class ActionData : IActionData
    {
        public AppAction Action { get; set; }
        public string ParameterValues { get; set; }
        public object DataObject { get; set; }

        public T GetDataValue<T>(string dataName) where T : class
        {
            var property = DataObject.GetType().GetProperty(dataName);
            if (property != null)
                return property.GetValue(DataObject, null) as T;
            return null;
        }

        public string GetDataValueAsString(string dataName)
        {
            var property = DataObject.GetType().GetProperty(dataName);
            return property != null ? property.GetValue(DataObject, null).ToString() : "";
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
