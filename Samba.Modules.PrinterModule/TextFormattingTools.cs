using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Modules.PrinterModule.Tools;

namespace Samba.Modules.PrinterModule
{
    static class TextFormattingTools
    {
        private static string GetTag(string line)
        {
            if (Regex.IsMatch(line, "<[^>]+>"))
            {
                var tag = Regex.Match(line, "<[^>]+>").Groups[0].Value;
                return tag;
            }
            return "";
        }

        public static IEnumerable<string> AlignLines(IEnumerable<string> lines, int maxWidth)
        {
            var columnWidths = CalculateColumnWidths(lines);
            var result = new List<string>();

            for (var i = 0; i < lines.Count(); i++)
            {
                var line = lines.ElementAt(i);
                if (line.Length < 4)
                {
                    result.Add(line);
                }
                else if (line.ToLower().StartsWith("<l"))
                {
                    result.Add(AlignLine(maxWidth, 0, line, LineAlignment.Left, false));
                }
                else if (line.ToLower().StartsWith("<r"))
                {
                    result.Add(AlignLine(maxWidth, 0, line, LineAlignment.Right, false));
                }
                else if (line.ToLower().StartsWith("<c"))
                {
                    result.Add(AlignLine(maxWidth, 0, line, LineAlignment.Center, false));
                }
                else if (line.ToLower().StartsWith("<j"))
                {
                    result.Add(AlignLine(maxWidth, 0, line, LineAlignment.Justify, false, columnWidths[0]));
                    if (i < lines.Count() - 1 && !lines.ElementAt(i + 1).ToLower().StartsWith("<j") && columnWidths.Count > 0)
                        columnWidths.RemoveAt(0);
                }
                else if (line.ToLower().StartsWith("<f>"))
                {
                    var c = line.Contains(">") ? line.Substring(line.IndexOf(">") + 1).Trim() : line.Trim();
                    if (c.Length == 1)
                        result.Add(c.PadLeft(maxWidth, c[0]));
                }
                else result.Add(line);
            }

            return result;
        }

        private static IList<int[]> CalculateColumnWidths(IEnumerable<string> lines)
        {
            var result = new List<int[]>();
            var tableNo = 0;
            foreach (var line in lines)
            {
                if (line.ToLower().StartsWith("<j"))
                {
                    var parts = line.Split('|');
                    if (tableNo == 0)
                    {
                        tableNo = result.Count + 1;
                        result.Add(new int[parts.Length]);
                    }

                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (result[tableNo - 1][i] < parts[i].Length)
                            result[tableNo - 1][i] = parts[i].Length;
                    }
                }
                else
                {
                    tableNo = 0;
                }
            }
            return result;
        }

        private static string AlignLine(int maxWidth, int width, string line, LineAlignment alignment, bool canBreak, int[] columnWidths = null)
        {
            maxWidth = maxWidth / (width + 1);

            var tag = GetTag(line);
            line = line.Replace(tag, "");

            switch (alignment)
            {
                case LineAlignment.Left:
                    return tag + line.PadRight(maxWidth, ' ');
                case LineAlignment.Right:
                    return tag + line.PadLeft(maxWidth, ' ');
                case LineAlignment.Center:
                    return tag + line.PadLeft(((maxWidth + line.Length) / 2), ' ');
                case LineAlignment.Justify:
                    return tag + JustifyText(maxWidth, line, canBreak, columnWidths);
                default:
                    return tag + line;
            }
        }

        private static string JustifyText(int maxWidth, string line, bool canBreak, IList<int> columnWidths = null)
        {
            var parts = line.Split('|');
            if (parts.Length == 1) return line;

            var text = "";
            for (var i = parts.Length - 1; i > 0; i--)
            {
                var l = columnWidths != null ? columnWidths[i] : parts[i].Length;
                parts[i] = parts[i].Trim().PadLeft(l);
                text = parts[i] + text;
            }

            if (parts[0].Length > maxWidth)
                parts[0] = parts[0].Substring(0, maxWidth);

            if (canBreak && parts[0].Length + text.Length > maxWidth)
            {
                return parts[0] + "\r" + text.PadLeft(maxWidth);
            }

            return parts[0].PadRight(maxWidth - text.Length).Substring(0, maxWidth - text.Length) + text;
        }
    }
}
