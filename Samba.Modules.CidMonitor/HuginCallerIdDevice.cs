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
    class HuginCallerIdDevice : AbstractCidDevice
    {
        private SerialPort _port;
        private GenericModemSettings _settings;
        public GenericModemSettings Settings { get { return _settings ?? (_settings = LoadSettings<GenericModemSettings>()); } }

        [ImportingConstructor]
        public HuginCallerIdDevice(IApplicationState applicationState, ICacheService cacheService, IEntityService entityService)
            : base(cacheService, applicationState, entityService)
        {

        }

        protected override DeviceType GetDeviceType()
        {
            return DeviceType.CallerId;
        }

        protected override string GetName()
        {
            return "Hugin Caller ID";
        }

        protected override bool DoInitialize()
        {
            try
            {
                _port = new SerialPort(Settings.PortName);
                _port.BaudRate = 38400;
                _port.RtsEnable = false;
                _port.DtrEnable = false;
                _port.Open();
                _port.DiscardOutBuffer();
                _port.DiscardInBuffer();
                _port.DataReceived += port_DataReceived;
            }
            catch (Exception e)
            {
                InteractionService.UserIntraction.DisplayPopup("", "Hugin Caller ID Error", e.Message);
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
            return !string.IsNullOrEmpty(Settings.MatchPattern) ? Settings.MatchPattern : "L.: .{10}([0-9]+)";
        }

        private string GetTerminateString()
        {
            return !string.IsNullOrEmpty(Settings.TerminateString) ? Settings.TerminateString : null;
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = !string.IsNullOrEmpty(GetTerminateString())
                ? _port.ReadTo(GetTerminateString())
                : _port.ReadTo("\r");
            var number = Regex.Match(data, GetMatchPattern()).Groups[1].Value;
            ProcessPhoneNumber(number);
        }
    }
}
