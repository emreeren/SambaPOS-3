using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class AppAction : Entity, IOrderable
    {
        public string ActionType { get; set; }

        private string _parameter;
        public string Parameter
        {
            get { return _parameter; }
            set
            {
                _parameter = value;
                _parameters = null;
            }
        }

        private Dictionary<string, string> _parameters;
        public Dictionary<string, string> Parameters
        {
            get { return _parameters ?? (_parameters = JsonHelper.Deserialize<Dictionary<string, string>>(Parameter)); }
        }

        public string GetParameter(string parameterName)
        {
            return Parameters.ContainsKey(parameterName) ? Parameters[parameterName] : "";
        }

        public string GetFormattedParameter(string parameterName, object dataObject, string parameterValues)
        {
            var format = GetParameter(parameterName);
            return !string.IsNullOrEmpty(format) && format.Contains("[") ? Format(format, dataObject, parameterValues) : format;
        }

        public string Format(string s, object dataObject, string parameterValues)
        {
            //var propertyNames = dataObject.GetType().GetProperties().Select(x => string.Format("[{0}]", x.Name)).ToList();

            //var parameters = (parameterValues ?? "").Split('#').Select(y => y.Split('='))
            //    .Where(x => x.Length == 2 && propertyNames.Contains(x[1]))
            //    .ToDictionary(x => x[0], x => dataObject.GetType().GetProperty(x[1].Trim('[', ']')).GetValue(dataObject, null));

            //foreach (var pVals in (parameterValues ?? "").Split('#').Select(p => p.Split('=')).Where(pVals => pVals.Length == 2 && !parameters.ContainsKey(pVals[0])))
            //{
            //    parameters.Add(pVals[0], pVals[1]);
            //}

            //var matches = Regex.Matches(s, "\\[([^\\]]+)\\]").Cast<Match>()
            //    .Select(match => match.Groups[1].Value)
            //    .Where(value => parameters.Keys.Contains(value));

            //return matches.Aggregate(s, (current, value) => current.Replace(string.Format("[{0}]", value), parameters[value].ToString()));

            if (!string.IsNullOrEmpty(parameterValues) && Regex.IsMatch(parameterValues, "\\[([^\\]]+)\\]"))
            {
                foreach (var propertyName in Regex.Matches(parameterValues, "\\[([^\\]]+)\\]").Cast<Match>().Select(match => match.Groups[1].Value).ToList())
                {
                    parameterValues = parameterValues.Replace(string.Format("[{0}]", propertyName),
                                             dataObject.GetType().GetProperty(propertyName).GetValue(dataObject, null).ToString());
                }
            }

            var parameters = (parameterValues ?? "")
                .Split('#')
                .Select(y => y.Split('='))
                .Where(x => x.Length > 1)
                .ToDictionary(x => x[0], x => x[1]);

            var matches = Regex.Matches(s, "\\[([^\\]]+)\\]").Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .Where(value => parameters.Keys.Contains(value));

            return matches.Aggregate(s, (current, value) => current.Replace(string.Format("[{0}]", value), parameters[value].ToString()));
        }

        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}
