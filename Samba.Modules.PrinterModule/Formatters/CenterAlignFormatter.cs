using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Modules.PrinterModule.Formatters
{
    class CenterAlignFormatter : AbstractLineFormatter
    {
        public CenterAlignFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            return Line.PadLeft(((MaxWidth + Line.Length)/2), ' ');
        }
    }
}
