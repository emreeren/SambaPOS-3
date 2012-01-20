using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Samba.Services.Implementations.SettingsModule
{
    public class SettingReplacer : ISettingReplacer
    {
        private readonly ProgramSettings _settings;

        public SettingReplacer(ProgramSettings settings)
        {
            _replaceCache = new Dictionary<string, string>();
            _settings = settings;
        }

        private readonly IDictionary<string, string> _replaceCache = new Dictionary<string, string>();

        public string ReplaceSettingValue(string template, string value)
        {
            // template = "\\{:[^}]+\\}"

            if (value == null) return "";
            while (Regex.IsMatch(value, template, RegexOptions.Singleline))
            {
                var match = Regex.Match(value, template);
                var tagName = match.Groups[0].Value;
                var settingName = match.Groups[1].Value;
                if (!_replaceCache.ContainsKey(settingName))
                    _replaceCache.Add(settingName, _settings.ReadSetting(settingName).StringValue);
                var settingValue = _replaceCache[settingName];
                value = value.Replace(tagName, settingValue);
            }
            return value;
        }
    }
}
