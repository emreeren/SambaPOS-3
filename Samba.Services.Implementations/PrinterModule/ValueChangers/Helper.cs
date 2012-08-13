using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    static class Helper
    {
        public static string FormatData(string data, string tag, Func<string> valueFunc)
        {
            if (!data.Contains(tag)) return data;

            var value = valueFunc.Invoke();
            var tagData = new TagData(data, tag);
            if (!string.IsNullOrEmpty(value)) value =
                !string.IsNullOrEmpty(tagData.Title) && tagData.Title.Contains("<value>")
                ? tagData.Title.Replace("<value>", value)
                : tagData.Title + value;
            return data.Replace(tagData.DataString, value);
        }
    }
}
