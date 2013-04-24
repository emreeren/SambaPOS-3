using System;
using System.Collections.Generic;

namespace Samba.Services.Common
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
            //if (parameterName.StartsWith("Is") && parameterName.Length > 2 && Char.IsUpper(parameterName[2]))
            //    return new[] { "True", "False" };
            return ParameterSource.ContainsKey(parameterName) ? ParameterSource[parameterName].Invoke() : new List<string>();
        }
    }
}
