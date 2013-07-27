using System.ComponentModel.Composition;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.SettingsModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class UpdateProgramSetting : ActionType
    {
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public UpdateProgramSetting(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public override void Process(ActionData actionData)
        {
            var settingName = actionData.GetAsString("SettingName");
            var updateType = actionData.GetAsString("UpdateType");
            if (!string.IsNullOrEmpty(settingName))
            {
                var isLocal = actionData.GetAsBoolean("IsLocal");
                var setting = isLocal
                    ? _settingService.ReadLocalSetting(settingName)
                    : _settingService.ReadGlobalSetting(settingName);

                if (updateType == Resources.Increase)
                {
                    var settingValue = actionData.GetAsInteger("SettingValue");
                    if (string.IsNullOrEmpty(setting.StringValue))
                        setting.IntegerValue = settingValue;
                    else
                        setting.IntegerValue = setting.IntegerValue + settingValue;
                }
                else if (updateType == Resources.Decrease)
                {
                    var settingValue = actionData.GetAsInteger("SettingValue");
                    if (string.IsNullOrEmpty(setting.StringValue))
                        setting.IntegerValue = settingValue;
                    else
                        setting.IntegerValue = setting.IntegerValue - settingValue;
                }
                else if (updateType == Resources.Toggle)
                {
                    var settingValue = actionData.GetAsString("SettingValue");
                    var parts = settingValue.Split(',');
                    if (string.IsNullOrEmpty(setting.StringValue))
                    {
                        setting.StringValue = parts[0];
                    }
                    else
                    {
                        for (var i = 0; i < parts.Length; i++)
                        {
                            if (parts[i] == setting.StringValue)
                            {
                                setting.StringValue = (i + 1) < parts.Length ? parts[i + 1] : parts[0];
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var settingValue = actionData.GetAsString("SettingValue");
                    setting.StringValue = settingValue;
                }
                if (!isLocal) _settingService.SaveProgramSettings();
            }
        }

        protected override object GetDefaultData()
        {
            return new { SettingName = "", SettingValue = "", UpdateType = Resources.Update, IsLocal = true };
        }

        protected override string GetActionName()
        {
            return Resources.UpdateProgramSetting;
        }

        protected override string GetActionKey()
        {
            return ActionNames.UpdateProgramSetting;
        }
    }
}
