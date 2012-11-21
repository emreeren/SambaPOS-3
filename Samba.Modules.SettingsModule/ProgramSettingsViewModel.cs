using System;
using System.ComponentModel.Composition;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ProgramSettingsViewModel : VisibleViewModelBase
    {
        public string WeightBarcodePrefix { get; set; }
        public int WeightBarcodeItemLength { get; set; }
        public string WeightBarcodeItemFormat { get; set; }
        public int WeightBarcodeQuantityLength { get; set; }
        public decimal AutoRoundDiscount { get; set; }
        public string PaymentScreenValues { get; set; }

        public ICaptionCommand SaveCommand { get; set; }

        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public ProgramSettingsViewModel(ISettingService settingService)
        {
            _settingService = settingService;
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave);
            WeightBarcodePrefix = _settingService.ProgramSettings.WeightBarcodePrefix;
            WeightBarcodeItemLength = _settingService.ProgramSettings.WeightBarcodeItemLength;
            WeightBarcodeItemFormat = _settingService.ProgramSettings.WeightBarcodeItemFormat;
            WeightBarcodeQuantityLength = _settingService.ProgramSettings.WeightBarcodeQuantityLength;
            AutoRoundDiscount = _settingService.ProgramSettings.AutoRoundDiscount;
            PaymentScreenValues = _settingService.ProgramSettings.PaymentScreenValues;
        }

        private void OnSave(object obj)
        {
            _settingService.ProgramSettings.WeightBarcodePrefix = WeightBarcodePrefix;
            _settingService.ProgramSettings.WeightBarcodeItemLength = WeightBarcodeItemLength;
            _settingService.ProgramSettings.WeightBarcodeQuantityLength = WeightBarcodeQuantityLength;
            _settingService.ProgramSettings.AutoRoundDiscount = AutoRoundDiscount;
            _settingService.ProgramSettings.WeightBarcodeItemFormat = WeightBarcodeItemFormat;
            _settingService.ProgramSettings.PaymentScreenValues = PaymentScreenValues;
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
