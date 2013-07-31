using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Samba.Services.Implementations.PrinterModule.Tools
{
    internal static class PrinterTools
    {
        public static string FlowDocumentToXaml(FlowDocument document)
        {
            var tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (var ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static FlowDocument XamlToFlowDocument(string text)
        {
            var document = new FlowDocument();
            using (var stream = new MemoryStream((new UTF8Encoding()).GetBytes(text)))
            {
                var txt = new TextRange(document.ContentStart, document.ContentEnd);
                txt.Load(stream, DataFormats.Xaml);
            }
            return document;
        }

        private static int _maxWidth;

        public static string[] FlowDocumentToSlipPrinterFormat(FlowDocument document, int maxWidth)
        {
            _maxWidth = maxWidth;
            var result = new List<string>();
            if (document != null)
                result.AddRange(ReadBlocks(document.Blocks));
            return result.ToArray();
        }

        private static IEnumerable<string> ReadBlocks(IEnumerable<Block> blocks)
        {
            var result = new List<string>();
            foreach (var block in blocks)
            {
                result.AddRange(ReadRuns(block));
                result.AddRange(ReadTables(block));
            }
            return result;
        }

        private static IEnumerable<string> ReadRuns(Block block)
        {
            var result = new List<string>();
            if (block is Paragraph)
            {
                result.AddRange((block as Paragraph).Inlines.OfType<Run>().Select(inline => inline.Text.Trim()));
            }
            return result;
        }

        private static IEnumerable<string> ReadTables(Block block)
        {
            var result = new List<string>();
            if (block is Table) result.AddRange(ReadTable(block as Table));
            return result;
        }

        private static IEnumerable<string> ReadTable(Table table)
        {
            var result = new List<string> { " " };
            var colLenghts = new int[table.Columns.Count];
            var colAlignments = new TextAlignment[table.Columns.Count];

            if (table.RowGroups.Count == 0) return result;

            foreach (var row in table.RowGroups[0].Rows)
            {
                for (var i = 0; i < row.Cells.Count; i++)
                {
                    if (table.RowGroups[0].Rows.Count > 1 && row == table.RowGroups[0].Rows[1])
                        colAlignments[i] = (row.Cells[i].Blocks.First()).TextAlignment;

                    var value = string.Join(" ", ReadBlocks(row.Cells[i].Blocks)).Trim();
                    if (value.Length > colLenghts[i] && row.Cells[0].ColumnSpan == 1)
                        colLenghts[i] = value.Length+2;
                }
            }

            if (_maxWidth > 0 && colLenghts.Sum() > _maxWidth)
            {
                while (colLenghts.Sum() > _maxWidth)
                    colLenghts[GetMaxCol(colLenghts)]--;
            }

            foreach (var row in table.RowGroups[0].Rows)
            {
                if (row == table.RowGroups[0].Rows[0]) result.Add("<EB>");

                var rowValue = "";
                for (var i = 0; i < row.Cells.Count; i++)
                {
                    var values = ReadBlocks(row.Cells[i].Blocks);

                    if (i == row.Cells.Count - 1 && row != table.RowGroups[0].Rows[0])
                    {
                        var v = string.Join(" ", values).Trim();
                        if (!string.IsNullOrEmpty(rowValue))
                            rowValue = rowValue + "|  " + v;
                        else rowValue = "<R>" + v;
                    }
                    else
                    {
                        var value = string.Join(" ", values);

                        if (value.Length > colLenghts[i] && row.Cells.Count > 1)
                            value = colLenghts[i] > 0 ? value.Substring(0, colLenghts[i] - 1) : "";

                        if (i < row.Cells.Count)
                        {
                            value = colAlignments[i] == TextAlignment.Right
                                ? row == table.RowGroups[0].Rows[0] ? value.PadLeft(colLenghts[i]) : "|  " + value
                                : value.PadRight(colLenghts[i]);
                        }

                        if (!string.IsNullOrEmpty(rowValue) && !rowValue.EndsWith(" ") && !value.StartsWith(" "))
                            value = " " + value;
                        rowValue += value;
                    }
                }


                if (row == table.RowGroups[0].Rows[0])
                {
                    result.Add("<L00>");
                    result.Add("<C00>" + rowValue);
                    result.Add("<DB>");
                    result.Add("<F>-");
                }
                else if (rowValue.Contains("|"))
                    result.Add("<J00>" + rowValue);
                else
                {
                    result.Add(rowValue);
                }
            }
            return result;
        }

        private static int GetMaxCol(IList<int> colLenghts)
        {
            var result = 0;
            for (var i = 1; i < colLenghts.Count; i++)
            {
                if (colLenghts[i] > colLenghts[result]) result = i;
            }
            return result;
        }
    }
}
