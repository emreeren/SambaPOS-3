using System.Collections.Generic;
using System.Linq;

namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    internal class JustifyAlignFormatter : AbstractLineFormatter
    {
        private readonly bool _canBreak;
        private readonly int[] _columnWidths;

        public JustifyAlignFormatter(string documentLine, int maxWidth, bool canBreak, int[] columnWidths = null) :
            base(documentLine, maxWidth)
        {
            _canBreak = canBreak;
            _columnWidths = CalculateColumnWidths(documentLine, columnWidths);
        }

        private static int[] CalculateColumnWidths(string documnentLine, int[] columnWidths)
        {
            var parts = documnentLine.Split('|');
            if (columnWidths == null || columnWidths.Count() != parts.Length)
                columnWidths = new int[parts.Count()];
            for (int i = 0; i < parts.Length; i++)
            {
                if (columnWidths[i] < parts[i].Length)
                    columnWidths[i] = parts[i].Length;
            }
            return columnWidths;
        }

        public override string GetFormattedLine()
        {
            return JustifyText(MaxWidth, Line, _canBreak, _columnWidths);
        }

        private static string JustifyText(int maxWidth, string line, bool canBreak, IList<int> columnWidths)
        {
            var parts = line.Split('|');
            if (parts.Length == 1) return line;

            var text = "";
            for (var i = parts.Length - 1; i > 0; i--)
            {
                var l = columnWidths[i]; //columnWidths != null ? columnWidths[i] : parts[i].Length;
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

        public int[] GetColumnWidths()
        {
            return _columnWidths;
        }
    }
}