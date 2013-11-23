using System;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Settings;
using Samba.Services.Common;

namespace Samba.Services.Implementations.SettingsModule
{
    public class ProgramSetting : IProgramSetting
    {
        private readonly ProgramSettingValue _programSetting;

        public ProgramSetting(ProgramSettingValue programSetting)
        {
            _programSetting = programSetting;
        }

        public string StringValue
        {
            get { return _programSetting.Value; }
            set { _programSetting.Value = value; }
        }

        public DateTime DateTimeValue
        {
            get
            {
                DateTime result;
                DateTime.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public int IntegerValue
        {
            get
            {
                int result;
                int.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public decimal DecimalValue
        {
            get
            {
                decimal result;
                decimal.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public bool BoolValue
        {
            get
            {
                bool result;
                bool.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        private static ProgramSetting _nullSetting;
        public static ProgramSetting NullSetting
        {
            get { return _nullSetting ?? (_nullSetting = new ProgramSetting(new ProgramSettingValue())); }
        }
    }
}
