using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Windows;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Helpers;
using Samba.Services.Common;

namespace Samba.Services.Implementations.PrinterModule.CustomPrinters
{
    public class UrlPrinterSettings
    {
        public string UrlFormat { get; set; }
        public bool LiveMode { get; set; }
    }

    [Export(typeof(ICustomPrinter))]
    class UrlPrinter : ICustomPrinter
    {
        public string Name { get { return "URL Printer"; } }
        public object GetSettingsObject(string customPrinterData)
        {
            return JsonHelper.Deserialize<UrlPrinterSettings>(customPrinterData);
        }

        public void Process(Printer printer, string document)
        {
            // https://www.voipbuster.com/myaccount/sendsms.php?username=USERNAME&password=PASS&from=FROM&to=@nummer@&text=@SMS@
            var settingsObject = GetSettingsObject(printer.CustomPrinterData) as UrlPrinterSettings;
            if (settingsObject == null) return;
            var result = document
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new[] { '=' }, 2))
                .Where(x => !string.IsNullOrEmpty(x[1]))
                .Aggregate(settingsObject.UrlFormat, (current, line) => current.Replace(string.Format("@{0}@", line[0]), line[1]));
            if (result.Contains("@")) return;
            if (settingsObject.LiveMode)
            {
                var c = new WebClient();
                c.DownloadDataAsync(new Uri(result));
            }
            else MessageBox.Show(result);
        }
    }
}
