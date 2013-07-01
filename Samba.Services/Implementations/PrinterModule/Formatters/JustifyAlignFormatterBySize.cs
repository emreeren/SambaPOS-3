using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    public class JustifyAlignFormatterBySize : JustifyAlignFormatter
    {
        public JustifyAlignFormatterBySize(string documentLine, int maxWidth, bool b, int[] lastColumnWidths)
            : base(documentLine, maxWidth, b, lastColumnWidths)
        {
        }

        protected override string[] Split(string line)
        {
            line = line.Replace("|", '\t' + "|");
            return base.Split(line);
        }

        protected override string Merge(int maxWidth, params string[] parts)
        {
            while (HaveSuitablePart(parts) && ActualLength(string.Join("", parts)) > maxWidth)
            {
                var index = GetSuitablePartIndex(parts);
                parts[index] = TrimPart(parts[index]);
            }
            return base.Merge(maxWidth, parts);
        }

        private string TrimPart(string part)
        {
            var index = part.IndexOf("  ", System.StringComparison.Ordinal);
            return part.Remove(index, 1);
        }

        private string TabifyPart(string part)
        {
            var index = part.IndexOf("  ", System.StringComparison.Ordinal);
            return part.Remove(index, 1).Insert(index, '\t' + "  ");
        }

        private bool HaveSuitablePart(IEnumerable<string> parts)
        {
            return parts.Any(x => x.EndsWith("  "));
        }

        private int GetSuitablePartIndex(string[] parts)
        {
            for (int i = 0; i < parts.Count(); i++)
            {
                if (parts[i].EndsWith("  ")) return i;
            }
            return -1;
        }

        public double ActualLength(string str)
        {
            double lenTotal = 0;
            var n = str.Length;
            for (var i = 0; i < n; i++)
            {
                var strWord = str.Substring(i, 1);
                int asc = Convert.ToChar(strWord);
                if (asc == 9)
                {
                    lenTotal = lenTotal + GetTabLength(lenTotal);
                    lenTotal = Math.Round(lenTotal);
                }
                else if (asc < 0 || asc > 127)
                    lenTotal = lenTotal + GetDifference(strWord);
                else
                    lenTotal = lenTotal + 1;
            }
            return lenTotal;
        }

        public double GetDifference(string c)
        {
            var nsize = GetSize("X");
            var ssize = GetSize(c);
            return ssize/nsize;
        }

        public int GetTabLength(double lenTotal)
        {
            var diff = Convert.ToInt32(lenTotal);
            if (lenTotal >= 8)
                diff = diff ^ 8;
            return 8 - diff;
        }

        public double GetSize(string text)
        {
            var v = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Lucida Console"), FontStyles.Normal, FontWeights.Normal, new FontStretch()), 12, Brushes.Black);
            return v.Extent;
        }

        public string GetMaxText(int maxWidth)
        {
            var result = Enumerable.Repeat("-", maxWidth).Aggregate("", (c, s) => c + s);
            return result;
        }
    }
}