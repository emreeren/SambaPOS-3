using System.Linq;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Services;

namespace Samba.Modules.PrinterModule
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
                    if (s.ToLower().StartsWith("<xct"))
                    {
                        SerialPortService.WriteCommand(Printer.ShareName, RemoveTag(s));
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
