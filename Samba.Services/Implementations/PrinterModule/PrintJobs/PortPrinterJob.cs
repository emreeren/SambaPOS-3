using System.Linq;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Services.Common;
using Samba.Services.Implementations.PrinterModule.Formatters;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    class PortPrinterJob : AbstractPrintJob
    {
        public PortPrinterJob(Printer printer)
            : base(printer)
        { }

        public override void DoPrint(string[] lines)
        {
            var document = new FormattedDocument(lines, Printer.CharsPerLine).GetFormattedDocument().ToArray();
            foreach (var line in document)
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
                    else if (s.ToLower().StartsWith("<xct"))
                    {
                        var lineData = s.ToLower().Replace("<xct", "").Trim(new[] { ' ', '<', '>' });
                        SerialPortService.WriteCommand(Printer.ShareName, lineData, Printer.CodePage);
                    }
                    else SerialPortService.WritePort(Printer.ShareName, RemoveTag(s), Printer.CodePage); 
                }
            }
            SerialPortService.ResetCache();
        }

        public override void DoPrint(FlowDocument document)
        {
            return;
        }
    }
}
