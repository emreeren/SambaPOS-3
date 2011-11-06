using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;
using Samba.Services.HtmlConverter;

namespace Samba.Services.Printing
{
    public class HtmlPrinterJob : AbstractPrintJob
    {
        public HtmlPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document));
        }

        public override void DoPrint(string[] lines)
        {
            var q = AppServices.PrintService.GetPrinter(Printer.ShareName);
            var formattedLines = ConvertTagsToHtml(lines);
            var text = formattedLines.Aggregate("", (current, s) => current + s + "\r\n");

            if (!text.ToLower().Contains("<style>"))
                text = LocalSettings.DefaultHtmlReportHeader + text;

            var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(text, false);

            PrintFlowDocument(q, PrinterTools.XamlToFlowDocument(xaml));
        }

        public IEnumerable<string> ConvertTagsToHtml(string[] lines)
        {
            var tables = new Dictionary<string, List<string>>();
            var lastLine = "";
            var tableCount = 0;

            var list = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("<F>") && line.Length > 3)
                    list.Add(string.Format("<span>{0}</span>", line[3].ToString().PadLeft(Printer.CharsPerLine, line[3])));
                if (line.StartsWith("<T>"))
                    list.Add(string.Format("<B>{0}</B>", RemoveTag(line)));
                if (line.StartsWith("<C") && line.Length > 3 && (line[2] == '>' || char.IsDigit(line[2])))
                    list.Add(string.Format("<Center>{0}</Center>", RemoveTag(line)));
                if (line.StartsWith("<L") && line.Length > 3 && (line[2] == '>' || char.IsDigit(line[2])))
                    list.Add(string.Format("<span>{0}</span>", RemoveTag(line)));
                if (line.StartsWith("<EB>"))
                    list.Add("<B>");
                if (line.StartsWith("<DB>"))
                    list.Add("</B>");

                if (line.StartsWith("<J") && line.Length > 3 && (line[2] == '>' || char.IsDigit(line[2])))
                {
                    if (!lastLine.StartsWith("<J"))
                    {
                        tableCount++;
                        list.Add("tbl" + tableCount);
                    }

                    var tableName = "tbl" + tableCount;
                    if (!tables.ContainsKey(tableName))
                        tables.Add(tableName, new List<string>());
                    tables[tableName].Add(RemoveTag(line));
                }

                if (!line.Contains("<"))
                    list.Add(line);

                lastLine = line;
            }

            foreach (var table in tables)
            {
                list.InsertRange(list.IndexOf(table.Key), GetTableLines(table.Value, Printer.CharsPerLine));
                list.Remove(table.Key);
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].TrimEnd();
                if ((!list[i].ToLower().EndsWith("<BR>") && RemoveTag(list[i]).Trim().Length > 0) || list[i].Trim().Length == 0)
                    list[i] += "<BR>";
                list[i] = list[i].Replace(" ", "&nbsp;");
            }

            return list;
        }

        private static IEnumerable<string> GetTableLines(IList<string> lines, int maxWidth)
        {
            int colCount = GetColumnCount(lines) + 1;
            var colWidths = new int[colCount];

            for (int i = 0; i < colCount; i++)
            {
                colWidths[i] = GetMaxLine(lines, i);
            }

            colWidths[colCount - 1] = (maxWidth - colWidths.Sum()) + colWidths[colCount - 1];
            if (colWidths[colCount - 1] < 1) colWidths[colCount - 1] = 1;

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = string.Format("<span>{0}</span>", GetFormattedLine(lines[i], colWidths));
            }

            return lines;
        }

        private static string GetFormattedLine(string s, IList<int> colWidths)
        {
            var parts = s.Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i == parts.Length - 1)
                    parts[i] = parts[i].PadLeft(colWidths[i]);
                else
                    parts[i] = parts[i].PadRight(colWidths[i]);
            }
            return string.Join("", parts);
        }

        private static int GetMaxLine(IEnumerable<string> lines, int columnNo)
        {
            var result = 0;
            foreach (var val in lines)
            {
                if (!val.Contains("|")) continue;
                int start = columnNo > 0 ? val.IndexOf("|", columnNo) : 0;
                var v = val.Substring(start);
                if (v.StartsWith("|")) v = v.Substring(1);
                if (v.Contains("|")) v = v.Substring(0, v.IndexOf("|"));
                result = v.Length + 1 > result ? v.Length + 1 : result;
            }
            return result;
        }

        private static int GetColumnCount(IEnumerable<string> value)
        {
            return value.Select(item => item.Length - item.Replace("|", "").Length).Aggregate(0, (current, len) => len > current ? len : current);
        }
    }
}
