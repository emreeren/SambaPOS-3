using Samba.Infrastructure.Data;

namespace Samba.Services
{
    public interface IExpressionService
    {
        string Eval(string expression);
        T Eval<T>(string expression, object dataObject, T defaultValue = default(T));
        T EvalCommand<T>(string functionName, IEntityClass entity, object dataObject, T defaultValue = default(T));
        string ReplaceExpressionValues(string data, string template = "\\[=([^\\]]+)\\]");
    }
}
