using System;
using System.Collections.Generic;
using Samba.Localization.Properties;

namespace Samba.Modules.AutomationModule
{
    public class RuleConstraintName
    {
        private readonly string _display;

        public RuleConstraintName(KeyValuePair<string, Type> data)
        {
            Type = data.Value;
            Name = data.Key;
            _display = Name;
            if (!Name.Contains(" "))
            {
                var result = Resources.ResourceManager.GetString(Name);
                if (!string.IsNullOrEmpty(result)) _display = result;
            }
        }

        public Type Type { get; set; }
        public string Name { get; set; }
        public string Display { get { return _display; } }
    }
}