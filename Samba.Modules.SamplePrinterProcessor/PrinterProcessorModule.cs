using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Presentation.Common;

namespace Samba.Modules.SamplePrinterProcessor
{
    [ModuleExport(typeof(PrinterProcessorModule))]
    public class PrinterProcessorModule : ModuleBase
    {
        
    }
}
