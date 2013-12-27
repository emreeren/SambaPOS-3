using System;
using System.ComponentModel.Composition;
using System.IO.Ports;
using System.Text.RegularExpressions;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.CidMonitor
{
    [Export(typeof(IDevice))]
    class GenericModemDevice : AbstractCidDevice
    {
        private SerialPort _port;
        private GenericModemSettings _settings;
        public GenericModemSettings Settings { get { return _settings ?? (_settings = LoadSettings<GenericModemSettings>()); } }

        [ImportingConstructor]
        public GenericModemDevice(IApplicationState applicationState, ICacheService cacheService, IEntityService entityService)
            : base(cacheService, applicationState, entityService)
        {

        }

        protected override DeviceType GetDeviceType()
        {
            return DeviceType.CallerId;
        }

        protected override string GetName()
        {
            return "Generic Modem";
        }

        protected override bool DoInitialize()
        {
            try
            {
                _port = new SerialPort(Settings.PortName);
                _port.Open();
                _port.DiscardOutBuffer();
                _port.DiscardInBuffer();
                _port.RtsEnable = true;
                var lines = GetInitializationString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    _port.WriteLine(line + "\r");
                }
                _port.DataReceived += port_DataReceived;
            }
            catch (Exception e)
            {
                InteractionService.UserIntraction.DisplayPopup("", "Generic Modem Error", e.Message);
                return false;
            }

            return true;
        }

        protected override void DoFinalize()
        {
            _port.DataReceived -= port_DataReceived;
            try
            {
                _port.Close();
            }
            finally
            {
                _port.Dispose();
                _port = null;
            }
        }

        protected override AbstractCidSettings GetSettings()
        {
            return Settings;
        }

        private string GetMatchPattern()
        {
            return !string.IsNullOrWhiteSpace(Settings.MatchPattern) ? Settings.MatchPattern : @"NMBR.*=?[^0-9]+([0-9]+)";
        }

        private string GetTerminateString()
        {
            return !string.IsNullOrWhiteSpace(Settings.TerminateString) ? Settings.TerminateString : "";
        }

        private string GetInitializationString()
        {
            return !string.IsNullOrWhiteSpace(Settings.InitializationString) ? Settings.InitializationString : "AT+VCID=0\r\nAT+VCID=1";
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = !string.IsNullOrEmpty(GetTerminateString())
                ? _port.ReadTo(GetTerminateString())
                : _port.ReadExisting();
            var number = Regex.Match(data, GetMatchPattern()).Groups[1].Value;
            ProcessPhoneNumber(number);
        }
    }
}
