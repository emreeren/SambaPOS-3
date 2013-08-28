using System;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    static class Helper
    {
        public static string FormatData(string data, string tag, Func<string> valueFunc)
        {
            if (!data.Contains(tag)) return data;
            var i = 0;
            while (data.Contains(tag) && i < 99)
            {
                var value = valueFunc.Invoke();
                var tagData = new TagData(data, tag);
                if (!string.IsNullOrEmpty(value) && tagData.DataString.StartsWith("[") && tagData.DataString.EndsWith("]") && value == 0.ToString("#,#0.00")) value = "";
                if (!string.IsNullOrEmpty(value)||data.Contains("\"")) 
                    value = !string.IsNullOrEmpty(tagData.Title) && tagData.Title.Contains("<value>")
                    ? tagData.Title.Replace("<value>", value)
                    : tagData.Title + value;
                var spos = data.IndexOf(tagData.DataString);
                data = data.Remove(spos, tagData.Length).Insert(spos, value ?? "");
                i++;
            }
            return data;
        }

        public static string FormatDataIf(bool condition, string data, string tag, Func<string> valueFunc)
        {
            if (condition && data.Contains(tag)) return FormatData(data, tag, valueFunc.Invoke);
            return RemoveTag(data, tag);
        }

        public static string RemoveTag(string data, string tag)
        {
            var tagData = new TagData(data, tag);
            return data.Replace(tagData.DataString, "");
        }
    }
}
