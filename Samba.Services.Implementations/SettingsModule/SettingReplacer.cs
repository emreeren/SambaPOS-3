using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Samba.Services.Implementations.SettingsModule
{
    public class SettingReplacer
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
            if (value == null) return "";
            var result = value;
            while (Regex.IsMatch(result, template, RegexOptions.Singleline))
            {
                var match = Regex.Match(result, template);
                var tagName = match.Groups[0].Value;
                var settingName = match.Groups[1].Value;
                if (!_replaceCache.ContainsKey(settingName))
                    _replaceCache.Add(settingName, _settings.ReadSetting(settingName).StringValue);
                var settingValue = _replaceCache[settingName];
                result = result.Replace(tagName, settingValue);
            }
            return result;
        }

        public void ClearCache()
        {
            _replaceCache.Clear();
        }
    }
}
