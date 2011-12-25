using Samba.Domain.Models.Actions;

namespace Samba.Services
{
    public interface IActionData
    {
        AppAction Action { get; set; }
        string ParameterValues { get; set; }
        object DataObject { get; set; }
        T GetDataValue<T>(string parameterName) where T : class;
        bool GetAsBoolean(string parameterName);
        string GetAsString(string parameterName);
        decimal GetAsDecimal(string parameterName);
        int GetAsInteger(string parameterName);
    }
}