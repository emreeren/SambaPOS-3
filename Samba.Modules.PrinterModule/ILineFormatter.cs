using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Modules.PrinterModule.Formatters;

namespace Samba.Modules.PrinterModule
{
    public interface ILineFormatter
    {
        int FontWidth { get; set; }
        int FontHeight { get; set; }
        FormatTag Tag { get; set; }
        string GetFormattedLine();
        string GetFormattedLineWithoutTags();
    }
}
