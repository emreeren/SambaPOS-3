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
    public class SettingPrinterSettings
    {
        public bool IsLocal { get; set; }
        public string SettingName { get; set; }
    }

    [Export(typeof(ICustomPrinter))]
    class SettingPrinter : ICustomPrinter
    {
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public SettingPrinter(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public string Name { get { return "Setting Printer"; } }
        public object GetSettingsObject(string customPrinterData)
        {
            return JsonHelper.Deserialize<SettingPrinterSettings>(customPrinterData);
        }

        public void Process(Printer printer, string document)
        {
            var settings = GetSettingsObject(printer.CustomPrinterData) as SettingPrinterSettings;
            if (settings == null) return;
            if (!string.IsNullOrEmpty(settings.SettingName))
            {
                var setting = settings.IsLocal
                    ? _settingService.ReadLocalSetting(settings.SettingName)
                    : _settingService.ReadGlobalSetting(settings.SettingName);
                setting.StringValue = document;
                if (!settings.IsLocal) _settingService.SaveProgramSettings();
            }
        }
    }
}
