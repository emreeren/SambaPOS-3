using PropertyTools.DataAnnotations;
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

        [FilePath(".*")]
        public string FileName { get; set; }

        public void Save()
        {
            _settingService.ReadGlobalSetting("SamplePrinterProcessorFileName").StringValue = FileName;
            _settingService.SaveProgramSettings();
        }

        public void Load()
        {
            FileName = _settingService.ReadGlobalSetting("SamplePrinterProcessorFileName").StringValue;
        }
    }
}
