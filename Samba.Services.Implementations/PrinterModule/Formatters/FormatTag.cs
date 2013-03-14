using System;

namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    public class FormatTag
    {
        public string Tag { get; set; }
        public string TagName { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public FormatTag(string data)
        {
            var tagEnd = data.IndexOf(">", System.StringComparison.Ordinal);
            Tag = tagEnd > 0 ? data.Substring(0, tagEnd + 1) : "";
            TagName = Tag.Trim(new[] { '<', '>' }).ToLower();
            if (Tag.Length > 4 && char.IsNumber(data[Tag.Length - 3]) && char.IsNumber(data[Tag.Length - 2]))
            {
                Height = Convert.ToInt32(Tag[Tag.Length - 3].ToString());
                Width = Convert.ToInt32(Tag[Tag.Length - 2].ToString());
                TagName = TagName.Substring(0, TagName.Length - 2);
            }
        }
    }
}