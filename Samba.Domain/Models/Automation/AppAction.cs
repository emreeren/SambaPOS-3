using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Automation
{
    public class AppAction : EntityClass, IOrderable
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

        public string GetFormattedParameter(string parameterName, dynamic dataObject, string parameterValues)
        {
            var format = GetParameter(parameterName);
            return !string.IsNullOrEmpty(format) && format.Contains("[") ? Format(format, dataObject, parameterValues) : format;
        }

        public string Format(string s, object dataObject, string parameterValues)
        {
            if (!string.IsNullOrEmpty(parameterValues) && Regex.IsMatch(parameterValues, "\\[:([^\\]]+)\\]"))
            {
                foreach (var propertyName in Regex.Matches(parameterValues, "\\[:([^\\]]+)\\]").Cast<Match>().Select(match => match.Groups[1].Value).ToList())
                {
                    var replace = "";
                    if (((IDictionary<string, object>)dataObject).ContainsKey(propertyName))
                        replace = SafeToString(((IDictionary<string, object>)dataObject)[propertyName]);
                    parameterValues = parameterValues.Replace(string.Format("[:{0}]", propertyName), replace);
                }
            }

            var parameters = (parameterValues ?? "")
                .Split('#')
                .Select(y => y.Split(new[] { '=' }, 2))
                .Where(x => x.Length > 1)
                .ToDictionary(x => x[0], x => x[1]);

            var matches = Regex.Matches(s, "\\[:([^\\]]+)\\]").Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .Where(value => parameters.Keys.Contains(value));

            s = matches.Aggregate(s, (current, value) => current.Replace(string.Format("[:{0}]", value), parameters[value].ToString()));

            var matches2 = Regex.Matches(s, "\\[:([^\\]]+)\\]").Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .Where(value => !parameters.Keys.Contains(value));

            return matches2.Aggregate(s, (current, value) => current.Replace(string.Format("[:{0}]", value), ""));

        }

        public string SafeToString(object o)
        {
            return o != null ? o.ToString() : "";
        }

        public int SortOrder { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}
