using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Modules.AutomationModule.ServiceImplementations
{
    internal static class ParameterSources
    {
        private static readonly IDictionary<string, Func<IEnumerable<string>>> ParameterSource;

        static ParameterSources()
        {
            ParameterSource = new Dictionary<string, Func<IEnumerable<string>>>();
        }

        public static void Add(string parameterName, Func<IEnumerable<string>> action)
        {
            ParameterSource.Add(parameterName, action);
        }

        public static IEnumerable<string> GetParameterSource(string parameterName)
        {
            return ParameterSource.ContainsKey(parameterName) ? ParameterSource[parameterName].Invoke() : new List<string>();
        }
    }
}
