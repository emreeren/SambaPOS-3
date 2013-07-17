using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Helpers;
using Samba.Services.Common;

namespace Samba.Services.Implementations.PrinterModule.CustomPrinters
{
    public class UrlPrinterSettings
    {
        public string UrlFormat { get; set; }
        public string DataFormat { get; set; }
        public string TokenChar { get; set; }
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
            var tokenChar = !string.IsNullOrEmpty(settingsObject.TokenChar) ? settingsObject.TokenChar : "@";
            var url = ReplaceValues(document, settingsObject.UrlFormat, tokenChar);
            if (url.Contains(tokenChar)) return;
            url = Uri.UnescapeDataString(url);

            var data = "";
            if (!string.IsNullOrEmpty(settingsObject.DataFormat))
            {
                data = ReplaceValues(document, settingsObject.DataFormat, tokenChar);
                if (data.Contains(tokenChar)) return;
                data = Uri.UnescapeDataString(data);
            }

            if (settingsObject.LiveMode)
            {
                var c = new WebClient();
                if (!string.IsNullOrEmpty(data))
                {
                    c.UploadDataAsync(new Uri(url), Encoding.GetEncoding(printer.CodePage).GetBytes(data));
                }
                else
                    c.DownloadDataAsync(new Uri(url));
            }
            else MessageBox.Show(url);
        }

        private static string ReplaceValues(string document, string format, string tokenFormat)
        {
            return document
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new[] { '=' }, 2))
                .Where(x => !string.IsNullOrEmpty(x[1]))
                .Aggregate(format, (current, line) => current.Replace(string.Format(tokenFormat + "{0}" + tokenFormat, line[0]), line[1]));
        }
    }
}
