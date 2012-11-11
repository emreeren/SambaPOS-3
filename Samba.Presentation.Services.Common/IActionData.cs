using Samba.Domain.Models.Automation;

namespace Samba.Presentation.Services.Common
{
    public interface IActionData
    {
        AppAction Action { get; set; }
        string ParameterValues { get; set; }
        object DataObject { get; set; }
        T GetDataValue<T>(string dataName) where T : class;
        string GetDataValueAsString(string dataName);
        int GetDataValueAsInt(string dataName);
        bool GetAsBoolean(string parameterName);
        string GetAsString(string parameterName);
        decimal GetAsDecimal(string parameterName);
        int GetAsInteger(string parameterName);
    }
}