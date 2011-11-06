using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class AppAction : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ActionType { get; set; }
        [StringLength(500)]
        public string Parameter { get; set; }

        public string GetParameter(string parameterName)
        {
            var param = Parameter.Split('#').Where(x => x.StartsWith(parameterName + "=")).FirstOrDefault();
            if (!string.IsNullOrEmpty(param) && param.Contains("=")) return param.Split('=')[1];
            return "";
        }

        public string GetFormattedParameter(string parameterName, object dataObject, string parameterValues)
        {
            var format = GetParameter(parameterName);
            return !string.IsNullOrEmpty(format) && format.Contains("[") ? Format(format, dataObject, parameterValues) : format;
        }

        public string Format(string s, object dataObject, string parameterValues)
        {
            var propertyNames = dataObject.GetType().GetProperties().Select(x => string.Format("[{0}]", x.Name)).ToList();

            var parameters = (parameterValues ?? "").Split('#').Select(y => y.Split('='))
                .Where(x => x.Length == 2 && propertyNames.Contains(x[1]))
                .ToDictionary(x => x[0], x => dataObject.GetType().GetProperty(x[1].Trim('[', ']')).GetValue(dataObject, null));

            foreach (var pVals in (parameterValues ?? "").Split('#').Select(p => p.Split('=')).Where(pVals => pVals.Length == 2 && !parameters.ContainsKey(pVals[0])))
            {
                parameters.Add(pVals[0], pVals[1]);
            }

            var matches = Regex.Matches(s, "\\[([^\\]]+)\\]").Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .Where(value => parameters.Keys.Contains(value));

            return matches.Aggregate(s, (current, value) => current.Replace(string.Format("[{0}]", value), parameters[value].ToString()));
        }
    }
}
