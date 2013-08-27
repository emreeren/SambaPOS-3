using System;
using System.Linq;

namespace Samba.Services.Implementations.PrinterModule
{
    public class TagData
    {
        public TagData(string data, string tag)
        {
            data = ReplaceInBracketValues(data, "\r\n", "<newline>", '[', ']');
            data = data
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(x => x.Contains(tag));

            Tag = tag;
            DataString = tag;
            if (string.IsNullOrEmpty(data)) return;

            StartPos = data.IndexOf(tag);
            EndPos = StartPos + 1;

            while (data[EndPos] != '}') { EndPos++; }
            EndPos++;
            Length = EndPos - StartPos;

            DataString = BracketContains(data, '[', ']', Tag) ? GetBracketValue(data, '[', ']') : data.Substring(StartPos, Length);
            DataString = DataString.Replace("<newline>", "\r\n");
            Title = !DataString.Contains("[=") && !DataString.Contains("\"") ? DataString.Trim('[', ']') : DataString;
            Title = Title.Replace(Tag, "<value>");
            Length = DataString.Length;
            StartPos = data.IndexOf(DataString);
            EndPos = StartPos + Length;
        }

        public string DataString { get; set; }
        public string Tag { get; set; }
        public string Title { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public int Length { get; set; }

        public static string ReplaceInBracketValues(string content, string find, string replace, char open, char close)
        {
            var result = content;
            var v1 = GetBracketValue(result, open, close);
            while (!string.IsNullOrEmpty(v1))
            {
                var value = v1.Replace(find, replace);
                value = value.Replace(open.ToString(), "<op>");
                value = value.Replace(close.ToString(), "<cl>");
                result = result.Replace(v1, value);
                v1 = GetBracketValue(result, open, close);
            }
            result = result.Replace("<op>", open.ToString());
            result = result.Replace("<cl>", close.ToString());
            return result;
        }

        public static bool BracketContains(string content, char open, char close, string testValue)
        {
            if (!content.Contains(open)) return false;
            var br = GetBracketValue(content, open, close);
            return (br.Contains(testValue)) && !br.StartsWith("[=");
        }

        public static string GetBracketValue(string content, char open, char close)
        {
            var closePass = 1;
            var start = content.IndexOf(open);
            var end = start;
            if (start > -1)
            {
                while (end < content.Length - 1 && closePass > 0)
                {
                    end++;
                    if (content[end] == open && close != open) closePass++;
                    if (content[end] == close) closePass--;
                }
                return content.Substring(start, (end - start) + 1);
            }
            return string.Empty;
        }
    }
}