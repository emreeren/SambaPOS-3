using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Helpers;
using Samba.Services.Common;

namespace Samba.Services.Implementations.PrinterModule.CustomPrinters
{
    public class SaveToFileSettings
    {
        public string FileName { get; set; }
    }

    [Export(typeof(ICustomPrinter))]
    class SaveToFilePrinter : ICustomPrinter
    {
        public string Name { get { return "Save to File Printer"; } }
        public object GetSettingsObject(string customPrinterData)
        {
            return JsonHelper.Deserialize<SaveToFileSettings>(customPrinterData);
        }

        public void Process(Printer printer, string document)
        {
            var settings = GetSettingsObject(printer.CustomPrinterData) as SaveToFileSettings;
            if (settings == null) return;
            if (!string.IsNullOrEmpty(settings.FileName))
            {
                File.WriteAllText(settings.FileName, document, Encoding.GetEncoding(printer.CodePage));
            }
        }
    }
}
