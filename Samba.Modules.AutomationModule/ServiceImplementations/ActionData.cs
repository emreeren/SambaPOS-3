using Samba.Domain.Models.Actions;
using Samba.Services;

namespace Samba.Modules.AutomationModule.ServiceImplementations
{
    public class ActionData : IActionData
    {
        public AppAction Action { get; set; }
        public string ParameterValues { get; set; }
        public object DataObject { get; set; }

        public T GetDataValue<T>(string parameterName) where T : class
        {
            var property = DataObject.GetType().GetProperty(parameterName);
            if (property != null)
                return property.GetValue(DataObject, null) as T;
            return null;
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
