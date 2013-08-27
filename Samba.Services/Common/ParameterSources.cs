using System;
using System.Collections.Generic;

namespace Samba.Services.Common
{
    public static class ParameterSources
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
            return !string.IsNullOrEmpty(parameterName) && ParameterSource.ContainsKey(parameterName) ? ParameterSource[parameterName].Invoke() : new List<string>();
        }
    }
}
