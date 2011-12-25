using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    public class ProgramSettingsViewModel : VisibleViewModelBase
    {
        public string WeightBarcodePrefix { get; set; }
        public int WeightBarcodeItemLength { get; set; }
        public string WeightBarcodeItemFormat { get; set; }
        public int WeightBarcodeQuantityLength { get; set; }
        public decimal AutoRoundDiscount { get; set; }

        public ICaptionCommand SaveCommand { get; set; }

        private readonly ISettingService _settingService;

        public ProgramSettingsViewModel()
        {
            _settingService = ServiceLocator.Current.GetInstance<ISettingService>();
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave);
            WeightBarcodePrefix = _settingService.ProgramSettings.WeightBarcodePrefix;
            WeightBarcodeItemLength = _settingService.ProgramSettings.WeightBarcodeItemLength;
            WeightBarcodeItemFormat = _settingService.ProgramSettings.WeightBarcodeItemFormat;
            WeightBarcodeQuantityLength = _settingService.ProgramSettings.WeightBarcodeQuantityLength;
            AutoRoundDiscount = _settingService.ProgramSettings.AutoRoundDiscount;
        }

        private void OnSave(object obj)
        {
            _settingService.ProgramSettings.WeightBarcodePrefix = WeightBarcodePrefix;
            _settingService.ProgramSettings.WeightBarcodeItemLength = WeightBarcodeItemLength;
            _settingService.ProgramSettings.WeightBarcodeQuantityLength = WeightBarcodeQuantityLength;
            _settingService.ProgramSettings.AutoRoundDiscount = AutoRoundDiscount;
            _settingService.ProgramSettings.WeightBarcodeItemFormat = WeightBarcodeItemFormat;
            _settingService.SaveProgramSettings();
            CommonEventPublisher.PublishViewClosedEvent(this);
        }

        protected override string GetHeaderInfo()
        {
            return Resources.ProgramSettings;
        }

        public override Type GetViewType()
        {
            return typeof(ProgramSettingsView);
        }
    }
}
