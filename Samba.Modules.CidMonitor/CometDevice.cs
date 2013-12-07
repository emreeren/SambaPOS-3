using System;
using System.ComponentModel.Composition;
using System.IO.Ports;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.CidMonitor
{
    [Export(typeof(IDevice))]
    class CometDevice : AbstractCidDevice
    {
        private SerialPort _port;
        private GenericModemSettings _settings;
        public GenericModemSettings Settings { get { return _settings ?? (_settings = LoadSettings<GenericModemSettings>()); } }
        int[] _buffer = new int[255];
        int _pointer;

        [ImportingConstructor]
        public CometDevice(IApplicationState applicationState, ICacheService cacheService, IEntityService entityService)
            : base(cacheService, applicationState, entityService)
        {

        }

        protected override DeviceType GetDeviceType()
        {
            return DeviceType.CallerId;
        }

        protected override string GetName()
        {
            return "CTI Comet USB Caller ID";
        }

        protected override bool DoInitialize()
        {
            try
            {
                var serialPort = new SerialPort();
                serialPort.PortName = Settings.PortName;
                serialPort.BaudRate = 1200;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Parity = Parity.None;
                serialPort.DataReceived += serialPort_DataReceived;
                serialPort.ReadTimeout = 100; // Required for end of packet Timeout notification
                serialPort.Open();
            }
            catch (Exception e)
            {
                InteractionService.UserIntraction.DisplayPopup("","Comet CID Error", e.Message);
                return false;
            }

            return true;
        }

        protected override void DoFinalize()
        {
            _port.DataReceived -= serialPort_DataReceived;
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

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                for (int c = 0; c != _port.BytesToRead; c++)
                {
                    _buffer[_pointer] = _port.ReadByte();
                    _pointer++;
                }
            }
            catch (TimeoutException x)
            {

                var cid = new CometData(_buffer);
                ProcessPhoneNumber(cid.getCIDNumber());
                _buffer = new int[255];
                _pointer = 0;
            }
        }
    }

    class CometData
    {
        // Caller ID packet variables
        const byte CALLER_ID = 0x80;
        const byte TIMEDATE = 0x01;
        const byte CALLING_NUMBER = 0x02;
        const byte WHY_NO_NUMBER = 0x04;
        const byte CALLING_NAME = 0x07;
        const byte CALL_TYPE = 0x11;

        // Caller ID storage variables
        private string CIDNumber;
        private string CIDName;


        // Variables
        private int i;
        private int bytes_remaining;

        public CometData(int[] data)
        {

            // Check Caller ID packet id
            if (data[0] == CALLER_ID)
            {
                bytes_remaining = data[1];
                i = 2;
                // While still bytes to process
                while (bytes_remaining > 0)
                {
                    int param_length = 0;
                    switch (data[i])
                    {
                        case TIMEDATE: bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            param_length = data[i]; // Read in length of this parameter
                            bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            // You would parse the time/date here.
                            // ..............
                            // Instead we are just discarding
                            bytes_remaining -= param_length; i += param_length;
                            break;

                        case CALLING_NUMBER: bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            param_length = data[i]; // Read in length of this parameter
                            bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            string number = "";
                            for (; param_length > 0; param_length--)
                            {
                                number += (char)data[i];
                                bytes_remaining--; i++; // move to next byte and decrease bytes left
                            }
                            CIDNumber = number;
                            break;

                        case CALLING_NAME: bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            param_length = data[i]; // Read in length of this parameter
                            bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            string name = "";
                            for (; param_length > 0; param_length--)
                            {
                                name += (char)data[i];
                                bytes_remaining--; i++; // move to next byte and decrease bytes left
                            }
                            CIDName = name;
                            break;

                        case WHY_NO_NUMBER: bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            param_length = data[i]; // Read in length of this parameter
                            bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            // You would parse the why no number here.
                            // ..............
                            // Instead we are just discarding
                            bytes_remaining -= param_length; i += param_length;
                            break;

                        case CALL_TYPE: bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            param_length = data[i]; // Read in length of this parameter
                            bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            // You would parse the call type here.
                            // ..............
                            // Instead we are just discarding
                            bytes_remaining -= param_length; i += param_length;
                            break;

                        // DISMISS ANY OTHER PARAMETERS
                        default: bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            param_length = data[i]; // Read in length of this parameter
                            bytes_remaining--; i++; // Move to next byte and decrease bytes left
                            bytes_remaining -= param_length; i += param_length;
                            break;

                    }
                }
            }
        }

        // Return the CID name
        public string getCIDName()
        {
            return CIDName;
        }

        // Return the CID number
        public string getCIDNumber()
        {
            return CIDNumber;
        }

    }
}
