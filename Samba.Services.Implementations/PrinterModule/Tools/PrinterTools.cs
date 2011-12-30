using System.Collections.Generic;
using System.IO;
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

        public static string[] FlowDocumentToSlipPrinterFormat(FlowDocument document)
        {
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
                result.AddRange((block as Paragraph).Inlines.OfType<Run>().Select(inline => inline.Text));
            }
            return result;
        }

        private static IEnumerable<string> ReadTables(Block block)
        {
            var result = new List<string>();
            if (block is Table)
            {
                result.AddRange(ReadTable(block as Table));
                result.Add("<C00> ");
            }
            return result;
        }

        private static IEnumerable<string> ReadTable(Table table)
        {
            var result = new List<string> { " " };
            var colLenghts = new int[table.Columns.Count];
            var colAlignments = new TextAlignment[table.Columns.Count];

            foreach (var row in table.RowGroups[0].Rows)
            {
                for (var i = 0; i < row.Cells.Count; i++)
                {
                    if (row == table.RowGroups[0].Rows[1])
                        colAlignments[i] = (row.Cells[i].Blocks.First()).TextAlignment;

                    var value = string.Join(" ", ReadBlocks(row.Cells[i].Blocks));
                    if (value.Length > colLenghts[i] && row.Cells[0].ColumnSpan == 1)
                        colLenghts[i] = value.Length;
                }
            }

            foreach (var row in table.RowGroups[0].Rows)
            {
                if (row == table.RowGroups[0].Rows[0]) result.Add("<EB>");

                var rowValue = "";
                for (var i = 0; i < row.Cells.Count; i++)
                {
                    var values = ReadBlocks(row.Cells[i].Blocks);

                    if (i == row.Cells.Count - 1 && row != table.RowGroups[0].Rows[0])
                        rowValue += " | " + string.Join(" ", values);
                    else
                    {
                        var value = string.Join(" ", values);

                        if (i < row.Cells.Count)
                        {
                            value = colAlignments[i] == TextAlignment.Right
                                ? value.PadLeft(colLenghts[i] + 1)
                                : value.PadRight(colLenghts[i] + 1);
                        }

                        rowValue += value;
                    }
                }


                if (row == table.RowGroups[0].Rows[0])
                {
                    result.Add("<C00>" + rowValue);
                    result.Add("<DB>");
                }
                else result.Add("<J00>" + rowValue);
            }
            return result;
        }
    }
}
