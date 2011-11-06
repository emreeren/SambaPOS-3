using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Printing
{
    class PortPrinterJob : AbstractPrintJob
    {
        public PortPrinterJob(Printer printer)
            : base(printer)
        { }

        public override void DoPrint(string[] lines)
        {
            foreach (var line in lines)
            {
                var data = line.Contains("<") ? line.Split('<').Where(x => !string.IsNullOrEmpty(x)).Select(x => '<' + x) : line.Split('#');
                foreach (var s in data)
                {
                    if (s.Trim().ToLower() == "<w>")
                        System.Threading.Thread.Sleep(100);
                    if (s.ToLower().StartsWith("<lb"))
                    {
                        SerialPortService.WritePort(Printer.ShareName, RemoveTag(s) + "\n\r");
                    }
                    else SerialPortService.WritePort(Printer.ShareName, RemoveTag(s));
                }
            }
        }

        public override void DoPrint(FlowDocument document)
        {
            return;
        }
    }
}
