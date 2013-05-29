using System;
using System.Collections.Generic;
using System.Reflection;
using Samba.Localization.Properties;

namespace Samba.Services.Common
{
    public class ParameterValue
    {
        private readonly string _parameterName;
        private readonly Type _paramaterType;

        public string Name { get { return _parameterName; } }
        public string NameDisplay
        {
            get
            {
                var result = Resources.ResourceManager.GetString(Name);
                return !string.IsNullOrEmpty(result) ? result + ":" : Name;
            }
        }

        public Type ValueType { get { return _paramaterType; } }
        public string Value { get; set; }

        private IEnumerable<string> _values;

        public ParameterValue(string parameterName, Type paramaterType)
        {
            _parameterName = parameterName;
            _paramaterType = paramaterType;
        }

        public IEnumerable<string> Values
        {
            get
            {
                if (ValueType == typeof(bool)) return new[] { "True", "False" };
                return _values ?? (_values = ParameterSources.GetParameterSource(Name));
            }
        }
    }
}