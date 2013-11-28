using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using FluentValidation.Results;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.SettingsModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class SettingsViewModel : VisibleViewModelBase
    {
        private readonly ISettingService _settingService;
        private readonly IMessagingService _messagingService;
        private readonly IDeviceService _deviceService;

        [ImportingConstructor]
        public SettingsViewModel(ISettingService settingService, IMessagingService messagingService, IDeviceService deviceService)
        {
            _settingService = settingService;
            _messagingService = messagingService;
            _deviceService = deviceService;
            SaveSettingsCommand = new CaptionCommand<string>(Resources.Save, OnSaveSettings);
            StartMessagingServerCommand = new CaptionCommand<string>(Resources.StartClientNow, OnStartMessagingServer, CanStartMessagingServer);
            DisplayCommonAppPathCommand = new CaptionCommand<string>(Resources.DisplayAppPath, OnDisplayAppPath);
            DisplayUserAppPathCommand = new CaptionCommand<string>(Resources.DisplayUserPath, OnDisplayUserPath);
            EditCallerIdDeviceSettingsCommand = new CaptionCommand<string>(Resources.Settings, OnEditCallerIdDeviceSettings);
        }

        public void OnDisplayUserPath(string obj)
        {
            var prc = new System.Diagnostics.Process { StartInfo = { FileName = LocalSettings.UserPath } };
            prc.Start();
        }

        public void OnDisplayAppPath(string obj)
        {
            var prc = new System.Diagnostics.Process { StartInfo = { FileName = LocalSettings.DataPath } };
            prc.Start();
        }

        private bool CanStartMessagingServer(string arg)
        {
            return _messagingService.CanStartMessagingClient();
        }

        private void OnStartMessagingServer(string obj)
        {
            _messagingService.StartMessagingClient();
        }

        private void OnSaveSettings(string obj)
        {
            LocalSettings.SaveSettings();
            _deviceService.FinalizeDevices();
            if (!string.IsNullOrEmpty(CallerIdDeviceName))
            {
                var device = _deviceService.GetDeviceByName(CallerIdDeviceName);
                if (device != null)
                {
                    device.SaveSettings();
                    _deviceService.InitializeDevice(CallerIdDeviceName);
                }
            }
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.LocalSettingsChanged);
            ((VisibleViewModelBase)this).PublishEvent(EventTopicNames.ViewClosed);
        }

        public ICaptionCommand SaveSettingsCommand { get; set; }
        public ICaptionCommand StartMessagingServerCommand { get; set; }
        public ICaptionCommand DisplayCommonAppPathCommand { get; set; }
        public ICaptionCommand DisplayUserAppPathCommand { get; set; }
        public ICaptionCommand EditCallerIdDeviceSettingsCommand { get; set; }

        public string TerminalName
        {
            get { return LocalSettings.TerminalName; }
            set { LocalSettings.TerminalName = value; }
        }

        public string ConnectionString
        {
            get { return LocalSettings.ConnectionString; }
            set { LocalSettings.ConnectionString = value; }
        }

        public string MessagingServerName
        {
            get { return LocalSettings.MessagingServerName; }
            set { LocalSettings.MessagingServerName = value; }
        }

        public int MessagingServerPort
        {
            get { return LocalSettings.MessagingServerPort; }
            set { LocalSettings.MessagingServerPort = value; }
        }

        public bool StartMessagingClient
        {
            get { return LocalSettings.StartMessagingClient; }
            set { LocalSettings.StartMessagingClient = value; }
        }

        public string CallerIdDeviceName
        {
            get { return LocalSettings.CallerIdDeviceName; }
            set { LocalSettings.CallerIdDeviceName = value; RaisePropertyChanged(() => CanEditDeviceSettings); }
        }

        public string WindowScale
        {
            get { return LocalSettings.WindowScale.Equals(0) ? "100" : (LocalSettings.WindowScale * 100).ToString(CultureInfo.CurrentCulture); }
            set { LocalSettings.WindowScale = Convert.ToDouble(value) / 100; }
        }

        public string Language
        {
            get { return LocalSettings.CurrentLanguage; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    LocalSettings.CurrentLanguage = "";
                }
                else if (LocalSettings.SupportedLanguages.Contains(value))
                {
                    LocalSettings.CurrentLanguage = value;
                }
                else
                {
                    var ci = CultureInfo.GetCultureInfo(value);
                    if (LocalSettings.SupportedLanguages.Contains(ci.TwoLetterISOLanguageName))
                    {
                        LocalSettings.CurrentLanguage = ci.TwoLetterISOLanguageName;
                    }
                }
            }
        }

        public bool OverrideWindowsRegionalSettings
        {
            get { return LocalSettings.OverrideWindowsRegionalSettings; }
            set
            {
                LocalSettings.OverrideWindowsRegionalSettings = value;
                RaisePropertyChanged(() => OverrideWindowsRegionalSettings);
            }
        }

        private IEnumerable<string> _terminalNames;
        public IEnumerable<string> TerminalNames
        {
            get { return _terminalNames ?? (_terminalNames = _settingService.GetTerminals().Select(x => x.Name)); }
        }

        private IEnumerable<CultureInfo> _supportedLanguages;
        public IEnumerable<CultureInfo> SupportedLanguages
        {
            get
            {
                return _supportedLanguages ?? (_supportedLanguages =
                    LocalSettings.SupportedLanguages.Select(CultureInfo.GetCultureInfo).ToList().OrderBy(x => x.NativeName));
            }
        }

        public IEnumerable<string> CallerIdDeviceNames { get { return _deviceService.GetDeviceNames(DeviceType.CallerId); } }

        public bool CanEditDeviceSettings { get { return !string.IsNullOrEmpty(CallerIdDeviceName); } }

        public void OnEditCallerIdDeviceSettings(string arg)
        {
            var device = _deviceService.GetDeviceByName(CallerIdDeviceName);
            if (device != null)
            {
                InteractionService.UserIntraction.EditProperties(device.GetSettingsObject());
            }
        }

        protected override string GetHeaderInfo()
        {
            return Resources.ProgramSettings;
        }

        public override Type GetViewType()
        {
            return typeof(SettingsView);
        }
    }
}
