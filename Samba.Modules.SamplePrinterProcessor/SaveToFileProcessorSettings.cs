using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Services;

namespace Samba.Modules.SamplePrinterProcessor
{
    public class SaveToFileProcessorSettings
    {
        private readonly ISettingService _settingService;

        public SaveToFileProcessorSettings(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public string FileName { get; set; }

        public void Save()
        {
            _settingService.ReadGlobalSetting("SamplePrinterProcessorFileName").StringValue = FileName;
        }

        public void Load()
        {
            FileName = _settingService.ReadGlobalSetting("SamplePrinterProcessorFileName").StringValue;
        }
    }
}
