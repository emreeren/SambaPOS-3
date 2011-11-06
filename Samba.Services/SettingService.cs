using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class SettingService
    {
        private readonly IDictionary<string, ProgramSetting> _settingCache = new Dictionary<string, ProgramSetting>();
        private readonly IDictionary<string, SettingGetter> _customSettingCache = new Dictionary<string, SettingGetter>();
        private IWorkspace _workspace;

        public SettingService()
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

        private SettingGetter _weightBarcodePrefix;
        private SettingGetter GetWeightBarcodePrefix()
        {
            return _weightBarcodePrefix ?? (_weightBarcodePrefix = GetSetting("WeightBarcodePrefix"));
        }

        private SettingGetter _autoRoundDiscount;
        private SettingGetter GetAutoRoundDiscount()
        {
            return _autoRoundDiscount ?? (_autoRoundDiscount = GetSetting("AutoRoundDiscount"));
        }

        private SettingGetter _weightBarcodeQuantityLength;
        private SettingGetter GetWeightBarcodeQuantityLength()
        {
            return _weightBarcodeQuantityLength ?? (_weightBarcodeQuantityLength = GetSetting("WeightBarcodeQuantityLength"));
        }

        private SettingGetter _weightBarcodeItemLength;
        private SettingGetter GetWeightBarcodeItemLength()
        {
            return _weightBarcodeItemLength ?? (_weightBarcodeItemLength = GetSetting("WeightBarcodeItemLength"));
        }

        private SettingGetter _weightBarcodeItemFormat;
        public SettingGetter GetWeightBarcodeItemFormat()
        {
            return _weightBarcodeItemFormat ?? (_weightBarcodeItemFormat = GetSetting("WeightBarcodeItemFormat"));
        }

        public SettingGetter GetCustomSetting(string settingName)
        {
            if (!_customSettingCache.ContainsKey(settingName))
                _customSettingCache.Add(settingName, GetSetting(settingName));
            return _customSettingCache[settingName];
        }

        public SettingGetter GetSetting(string valueName)
        {
            var setting = _workspace.Single<ProgramSetting>(x => x.Name == valueName);
            if (_settingCache.ContainsKey(valueName))
            {
                if (setting == null)
                    setting = _settingCache[valueName];
                else _settingCache.Remove(valueName);
            }
            if (setting == null)
            {
                setting = new ProgramSetting { Name = valueName };
                _settingCache.Add(valueName, setting);
                _workspace.Add(setting);
            }
            return new SettingGetter(setting);
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
