using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class PrinterTemplate : Entity
    {
        private string _template;
        public string Template
        {
            get { return _template; }
            set
            {
                _template = value;
                _parts = null;
            }
        }

        public bool MergeLines { get; set; }

        private IDictionary<string, StringBuilder> _parts;
        public IDictionary<string, StringBuilder> Parts
        {
            get { return _parts ?? (_parts = CreateParts(Template)); }
        }

        public string GetPart(string partName)
        {
            if (string.IsNullOrEmpty(partName)) partName = "LAYOUT";
            partName = partName.ToUpper(CultureInfo.InvariantCulture);
            if (Parts.ContainsKey(partName)) return Parts[partName].ToString();
            var p2 = partName.Contains(":") ? partName.Substring(0, partName.IndexOf(':')) : "";
            if (Parts.ContainsKey(p2)) return Parts[p2].ToString();
            return "";
        }

        private IDictionary<string, StringBuilder> CreateParts(string template)
        {
            var result = new Dictionary<string, StringBuilder>();
            var currentSection = "LAYOUT";
            result.Add(currentSection, new StringBuilder());
            foreach (var line in template.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var m = Regex.Match(line, @"(?<=\[)(?<SectionName>[^\]]+)(?=\])");
                if (m.Success && !line.Contains("<"))
                {
                    currentSection = m.Groups["SectionName"].Value.ToUpper(CultureInfo.InvariantCulture);
                    if (!result.ContainsKey(currentSection))
                        result.Add(currentSection, new StringBuilder());
                }
                else
                {
                    if (!line.StartsWith("-- ") && result.ContainsKey(currentSection))
                        result[currentSection].AppendLine(line);
                }
            }
            return result;
        }
    }
}
