using System;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    class FunctionData<T>
    {
        public string Tag { get; set; }
        public Func<T, string, string> Func { get; set; }
        public Func<T, bool> Condition { get; set; }

        public string GetResult(T model, string currentData, PrinterTemplate template)
        {
            var t = Tag;
            var tagValue = "";
            if (Tag.Contains(":") && Regex.IsMatch(currentData, Tag))
            {
                var m = Regex.Match(currentData, Tag);
                tagValue = m.Groups[1].Value.Trim();
                t = m.Groups[0].Value;
            }

            while (currentData.Contains(t))
                currentData = FormatDataIf(Condition == null || Condition.Invoke(model), currentData, t, () => Func.Invoke(model, tagValue));
            return currentData;
        }

        protected string FormatDataIf(bool condition, string data, string tag, Func<string> valueFunc)
        {
            if (condition && data.Contains(tag)) return Helper.FormatData(data, tag, valueFunc.Invoke);
            return RemoveTag(data, tag);
        }

        protected string RemoveTag(string data, string tag)
        {
            var tagData = new TagData(data, tag);
            return data.Replace(tagData.DataString, "");
        }
    }
}