using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export]
    internal class Preprocessor
    {
        private readonly ISettingService _settingService;
        private readonly IExpressionService _expressionService;
        private readonly IPrinterService _printerService;

        [ImportingConstructor]
        public Preprocessor(ISettingService settingService, IExpressionService expressionService, IPrinterService printerService)
        {
            _settingService = settingService;
            _expressionService = expressionService;
            _printerService = printerService;
        }

        public string Process(string input, object dataObject)
        {
            _settingService.ClearSettingCache();
            var result = _settingService.ReplaceSettingValues(input);
            if (dataObject != null)
            {
                result = ReplaceParameterValues(result, dataObject);
                result = ReplaceModelData(result, dataObject);
                result = _expressionService.ReplaceExpressionValues(result, dataObject);
            }
            return result;
        }

        private string ReplaceModelData(string data, object dataObject)
        {
            if (string.IsNullOrEmpty(data)) return "";
            if (!data.Contains("{")) return data;
            var result = data;
            result = ProcessModelData<Ticket>(result, dataObject);
            result = ProcessModelData<Entity>(result, dataObject);
            result = ProcessModelData<Order>(result, dataObject);
            return result;
        }

        private string ProcessModelData<T>(string data, object dataObject) where T : class
        {
            var model = GetData<T>(dataObject, typeof(T).Name);
            if (model != null)
            {
                return _printerService.ExecuteFunctions(data, model);
            }
            return data;
        }

        private string ReplaceParameterValues(string parameterValues, object dataObject)
        {
            if (!string.IsNullOrEmpty(parameterValues) && Regex.IsMatch(parameterValues, "\\[:([^\\]]+)\\]"))
            {
                foreach (var propertyName in Regex.Matches(parameterValues, "\\[:([^\\]]+)\\]").Cast<Match>().Select(match => match.Groups[1].Value).ToList())
                {
                    if (!ContainsData(dataObject, propertyName)) continue;
                    var val = ((IDictionary<string, object>)dataObject)[propertyName];
                    parameterValues = parameterValues.Replace(string.Format("[:{0}]", propertyName), val != null ? val.ToString() : "");
                }
            }
            return parameterValues;
        }

        public bool ContainsData(object dataObject, string propertyName)
        {
            return ((IDictionary<string, object>)dataObject).ContainsKey(propertyName);
        }

        public T GetData<T>(object dataObject, string propertyName) where T : class
        {
            if (ContainsData(dataObject, propertyName))
            {
                return ((IDictionary<string, object>)dataObject)[propertyName] as T;
            }
            return default(T);
        }
    }
}