using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;

namespace Samba.Services.Implementations.SettingsModule
{
    public class ProgramSettings : IProgramSettings
    {
        private readonly IDictionary<string, ProgramSettingValue> _settingCache = new Dictionary<string, ProgramSettingValue>();
        private readonly IDictionary<string, ProgramSetting> _customSettingCache = new Dictionary<string, ProgramSetting>();
        private IWorkspace _workspace;

        public ProgramSettings()
        {
            _workspace = WorkspaceFactory.Create();
        }

        public string WeightBarcodePrefix
        {
            get { return GetWeightBarcodePrefix().StringValue; }
            set { GetWeightBarcodePrefix().StringValue = value; }
        }

        public int WeightBarcodeItemLength
        {
            get { return GetWeightBarcodeItemLength().IntegerValue; }
            set { GetWeightBarcodeItemLength().IntegerValue = value; }
        }

        public string WeightBarcodeItemFormat
        {
            get { return GetWeightBarcodeItemFormat().StringValue; }
            set { GetWeightBarcodeItemFormat().StringValue = value; }
        }

        public int WeightBarcodeQuantityLength
        {
            get { return GetWeightBarcodeQuantityLength().IntegerValue; }
            set { GetWeightBarcodeQuantityLength().IntegerValue = value; }
        }

        public decimal AutoRoundDiscount
        {
            get { return GetAutoRoundDiscount().DecimalValue; }
            set { GetAutoRoundDiscount().DecimalValue = value; }
        }

        private ProgramSetting _weightBarcodePrefix;
        private ProgramSetting GetWeightBarcodePrefix()
        {
            return _weightBarcodePrefix ?? (_weightBarcodePrefix = GetSetting("WeightBarcodePrefix"));
        }

        private ProgramSetting _autoRoundDiscount;
        private ProgramSetting GetAutoRoundDiscount()
        {
            return _autoRoundDiscount ?? (_autoRoundDiscount = GetSetting("AutoRoundDiscount"));
        }

        private ProgramSetting _weightBarcodeQuantityLength;
        private ProgramSetting GetWeightBarcodeQuantityLength()
        {
            return _weightBarcodeQuantityLength ?? (_weightBarcodeQuantityLength = GetSetting("WeightBarcodeQuantityLength"));
        }

        private ProgramSetting _weightBarcodeItemLength;
        private ProgramSetting GetWeightBarcodeItemLength()
        {
            return _weightBarcodeItemLength ?? (_weightBarcodeItemLength = GetSetting("WeightBarcodeItemLength"));
        }

        private ProgramSetting _weightBarcodeItemFormat;
        private ProgramSetting GetWeightBarcodeItemFormat()
        {
            return _weightBarcodeItemFormat ?? (_weightBarcodeItemFormat = GetSetting("WeightBarcodeItemFormat"));
        }

        public ProgramSetting GetCustomSetting(string settingName)
        {
            if (!_customSettingCache.ContainsKey(settingName))
                _customSettingCache.Add(settingName, GetSetting(settingName));
            return _customSettingCache[settingName];
        }

        public ProgramSetting GetSetting(string valueName)
        {
            var setting = _workspace.Single<ProgramSettingValue>(x => x.Name == valueName);
            if (_settingCache.ContainsKey(valueName))
            {
                if (setting == null)
                    setting = _settingCache[valueName];
                else _settingCache.Remove(valueName);
            }
            if (setting == null)
            {
                setting = new ProgramSettingValue { Name = valueName };
                _settingCache.Add(valueName, setting);
                _workspace.Add(setting);
            }
            return new ProgramSetting(setting);
        }

        public void SaveChanges()
        {
            _workspace.CommitChanges();
        }

        public void ResetCache()
        {
            _workspace = WorkspaceFactory.Create();
            _customSettingCache.Clear();
        }
    }
}
