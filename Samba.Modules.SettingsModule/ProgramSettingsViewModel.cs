using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ProgramSettingsViewModel()
        {
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave);
            WeightBarcodePrefix = AppServices.SettingService.WeightBarcodePrefix;
            WeightBarcodeItemLength = AppServices.SettingService.WeightBarcodeItemLength;
            WeightBarcodeItemFormat = AppServices.SettingService.WeightBarcodeItemFormat;
            WeightBarcodeQuantityLength = AppServices.SettingService.WeightBarcodeQuantityLength;
            AutoRoundDiscount = AppServices.SettingService.AutoRoundDiscount;
        }

        private void OnSave(object obj)
        {
            AppServices.SettingService.WeightBarcodePrefix = WeightBarcodePrefix;
            AppServices.SettingService.WeightBarcodeItemLength = WeightBarcodeItemLength;
            AppServices.SettingService.WeightBarcodeQuantityLength = WeightBarcodeQuantityLength;
            AppServices.SettingService.AutoRoundDiscount = AutoRoundDiscount;
            AppServices.SettingService.WeightBarcodeItemFormat = WeightBarcodeItemFormat;
            AppServices.SettingService.SaveChanges();
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
