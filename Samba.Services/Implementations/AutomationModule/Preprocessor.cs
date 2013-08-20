using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export]
    internal class Preprocessor
    {
        private readonly ISettingService _settingService;
        private readonly IExpressionService _expressionService;

        [ImportingConstructor]
        public Preprocessor(ISettingService settingService, IExpressionService expressionService)
        {
            _settingService = settingService;
            _expressionService = expressionService;
        }

        public string Process(string input, object dataObject)
        {
            _settingService.ClearSettingCache();
            var result = _settingService.ReplaceSettingValues(input);
            if (dataObject != null)
            {
                result = ReplaceParameterValues(result, dataObject);
                result = _expressionService.ReplaceExpressionValues(result, dataObject);
            }
            return result;
        }

        private string ReplaceParameterValues(string parameterValues, object dataObject)
        {
            if (!string.IsNullOrEmpty(parameterValues) && Regex.IsMatch(parameterValues, "\\[:([^\\]]+)\\]"))
            {
                foreach (var propertyName in Regex.Matches(parameterValues, "\\[:([^\\]]+)\\]").Cast<Match>().Select(match => match.Groups[1].Value).ToList())
                {
                    if (!((IDictionary<string, object>)dataObject).ContainsKey(propertyName)) continue;
                    var val = ((IDictionary<string, object>)dataObject)[propertyName];
                    parameterValues = parameterValues.Replace(string.Format("[:{0}]", propertyName), val != null ? val.ToString() : "");
                }
            }
            return parameterValues;
        }
    }
}