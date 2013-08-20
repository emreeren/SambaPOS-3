using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.SettingsModule
{
    public class ProgramSettings : IProgramSettings
    {
        private readonly IDictionary<string, ProgramSettingValue> _settingCache = new Dictionary<string, ProgramSettingValue>();
        private readonly IDictionary<string, ProgramSetting> _customSettingCache = new Dictionary<string, ProgramSetting>();
        private IWorkspace _workspace;
        private IWorkspace Workspace { get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); } }

        public string QuantitySeparators
        {
            get { return GetQuantitySeparators().StringValue ?? "x,X"; }
            set { GetQuantitySeparators().StringValue = value; }
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

        public string PaymentScreenValues
        {
            get { return GetPaymentScreenValues().StringValue; }
            set { GetPaymentScreenValues().StringValue = value; }
        }

        public string UserInfo
        {
            get { return GetUserInfo().StringValue; }
            set { GetUserInfo().StringValue = value; }
        }

        private ProgramSetting _userInfo;
        public ProgramSetting GetUserInfo()
        {
            return _userInfo ?? (_userInfo = GetSetting("UserInfo"));
        }

        private ProgramSetting _quantitySeparators;
        private ProgramSetting GetQuantitySeparators()
        {
            return _quantitySeparators ?? (_quantitySeparators = GetSetting("QuantitySeparators"));
        }

        private ProgramSetting _paymentScreenValues;
        public ProgramSetting GetPaymentScreenValues()
        {
            return _paymentScreenValues ?? (_paymentScreenValues = GetSetting("PaymentScreenValues"));
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

        public ProgramSetting ReadLocalSetting(string settingName)
        {
            if (!_customSettingCache.ContainsKey(settingName))
            {
                var p = new ProgramSettingValue { Name = settingName };
                var getter = new ProgramSetting(p);
                _customSettingCache.Add(settingName, getter);
            }
            return _customSettingCache[settingName];
        }

        public ProgramSetting ReadGlobalSetting(string settingName)
        {
            return GetSetting(settingName);
        }

        public ProgramSetting ReadSetting(string settingName)
        {
            if (_customSettingCache.ContainsKey(settingName))
                return _customSettingCache[settingName];
            if (_settingCache.ContainsKey(settingName))
                return new ProgramSetting(_settingCache[settingName]);
            var setting = Dao.Single<ProgramSettingValue>(x => x.Name == settingName); //_workspace.Single<ProgramSetting>(x => x.Name == settingName);)
            return setting != null ? new ProgramSetting(setting) : ProgramSetting.NullSetting;
        }

        public ProgramSetting GetSetting(string valueName)
        {
            var setting = Workspace.Single<ProgramSettingValue>(x => x.Name == valueName);
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
                Workspace.Add(setting);
            }
            return new ProgramSetting(setting);
        }

        public void SaveChanges()
        {
            Workspace.CommitChanges();
            ResetCache();
        }

        public void ResetCache()
        {
            _workspace = null;
            _customSettingCache.Clear();
            _settingCache.Clear();
            _paymentScreenValues = null;
            _weightBarcodePrefix = null;
            _autoRoundDiscount = null;
            _weightBarcodeQuantityLength = null;
            _weightBarcodeItemLength = null;
            _weightBarcodeItemFormat = null;
        }
    }
}
