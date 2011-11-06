using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Samba.Modules.BasicReports
{
    public class SimpleReport
    {
        private readonly LengthConverter _lengthConverter = new LengthConverter();
        private readonly GridLengthConverter _gridLengthConverter = new GridLengthConverter();

        public FlowDocument Document { get; set; }
        public Paragraph Header { get; set; }
        public IDictionary<string, Table> Tables { get; set; }
        public IDictionary<string, GridLength[]> ColumnLengths { get; set; }
        public IDictionary<string, TextAlignment[]> ColumnTextAlignments { get; set; }

        public SimpleReport(string pageWidth)
        {
            Tables = new Dictionary<string, Table>();
            ColumnLengths = new Dictionary<string, GridLength[]>();
            ColumnTextAlignments = new Dictionary<string, TextAlignment[]>();
            Header = new Paragraph { TextAlignment = TextAlignment.Center, FontSize = 14 };
            Document = new FlowDocument(Header)
                           {
                               ColumnGap = 20.0,
                               ColumnRuleBrush = Brushes.DodgerBlue,
                               ColumnRuleWidth = 2.0,
                               PageWidth = StringToLength("10cm"),

                               //ColumnWidth = StringToLength("6cm"),
                               FontFamily = new FontFamily("Segoe UI")
                           };
        }

        public void AddColumnLength(string tableName, params string[] values)
        {
            if (!ColumnLengths.ContainsKey(tableName))
                ColumnLengths.Add(tableName, new GridLength[0]);

            ColumnLengths[tableName] = values.Select(StringToGridLength).ToArray();
        }

        public void AddColumTextAlignment(string tableName, params TextAlignment[] values)
        {
            if (!ColumnTextAlignments.ContainsKey(tableName))
                ColumnTextAlignments.Add(tableName, new TextAlignment[0]);
            ColumnTextAlignments[tableName] = values;
        }

        public void AddTable(string tableName, params string[] headers)
        {
            var table = new Table
                            {
                                CellSpacing = 0,
                                BorderThickness = new Thickness(0.5, 0.5, 0, 0),
                                BorderBrush = Brushes.Black

                            };

            Document.Blocks.Add(table);
            Tables.Add(tableName, table);

            var lengths = ColumnLengths.ContainsKey(tableName)
                ? ColumnLengths[tableName]
                : new[] { GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star) };

            for (var i = 0; i < headers.Count(); i++)
            {
                var c = new TableColumn { Width = lengths[i] };
                table.Columns.Add(c);
            }

            var rows = new TableRowGroup();
            table.RowGroups.Add(rows);
            rows.Rows.Add(CreateRow(headers, new[] { TextAlignment.Center }, true));
        }

        public void AddHeader(string text)
        {
            AddNewLine(Header, text, true);
        }

        public void AddRow(string tableName, params object[] values)
        {
            Tables[tableName].RowGroups[0].Rows.Add(CreateRow(values, ColumnTextAlignments.ContainsKey(tableName) ? ColumnTextAlignments[tableName] : new[] { TextAlignment.Left }, false));
        }

        public void AddBoldRow(string tableName, params object[] values)
        {
            Tables[tableName].RowGroups[0].Rows.Add(CreateRow(values, ColumnTextAlignments.ContainsKey(tableName) ? ColumnTextAlignments[tableName] : new[] { TextAlignment.Left }, true));
        }

        private static void AddNewLine(Paragraph p, string text, bool bold)
        {
            p.Inlines.Add(new Run(text) { FontWeight = bold ? FontWeights.Bold : FontWeights.Normal });
            p.Inlines.Add(new LineBreak());
        }

        public void AddLink(string text)
        {
            var hp = new Hyperlink(new Run(text)) { Name = text.Replace(" ", "_") };
            Header.Inlines.Add(hp);
            Header.Inlines.Add(new LineBreak());
        }

        public TableRow CreateRow(object[] values, TextAlignment[] alignment, bool bold)
        {
            var row = new TableRow();
            TableCell lastCell = null;
            int index = 0;
            foreach (var value in values)
            {
                var val = value ?? "";
                var r = new Run(val.ToString()) { FontWeight = bold ? FontWeights.Bold : FontWeights.Normal };
                if (string.IsNullOrEmpty(val.ToString()) && lastCell != null)
                    lastCell.ColumnSpan++;
                else
                {
                    var p = new Paragraph(r);
                    p.FontSize = 14;
                    p.TextAlignment = alignment.Length <= index ? alignment[alignment.Length - 1] : alignment[index];
                    lastCell = new TableCell(p)
                                   {
                                       BorderBrush = Brushes.Black,
                                       BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                                       Padding = new Thickness(3),
                                       Background = Brushes.Snow
                                   };
                    if (bold)
                    {
                        lastCell.Foreground = Brushes.White;
                        lastCell.Background = Brushes.Gray;
                    }

                    row.Cells.Add(lastCell);
                }
                index++;
            }

            return row;
        }

        private GridLength StringToGridLength(string value)
        {
            return (GridLength)_gridLengthConverter.ConvertFromString(value);
        }

        private double StringToLength(string value)
        {
            return (double)_lengthConverter.ConvertFromString(value);
        }


    }
}
