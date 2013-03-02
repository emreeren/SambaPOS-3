﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services.Implementations.AutomationModule
{
    internal class ParameterValue : IParameterValue
    {
        private readonly PropertyInfo _parameterInfo;

        public string Name { get { return _parameterInfo.Name; } }
        public string NameDisplay
        {
            get
            {
                var result = Resources.ResourceManager.GetString(Name);
                return !string.IsNullOrEmpty(result) ? result + ":" : Name;
            }
        }

        public Type ValueType { get { return _parameterInfo.PropertyType; } }
        public string Value { get; set; }

        private IEnumerable<string> _values;

        public ParameterValue(PropertyInfo propertyInfo)
        {
            _parameterInfo = propertyInfo;
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