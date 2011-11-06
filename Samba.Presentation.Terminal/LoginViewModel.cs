using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Presentation.Terminal
{
    public delegate void PinSubmittedEventHandler(object sender, string pinValue);

    public class LoginViewModel : ObservableObject
    {
        public event PinSubmittedEventHandler PinSubmitted;

        public void InvokePinSubmitted(string pinvalue)
        {
            var handler = PinSubmitted;
            if (handler != null) handler(this, pinvalue);
        }

        private string _pin = "";

        public string PinDisplay
        {
            get { return !string.IsNullOrEmpty(_pin) ? "*".PadLeft(_pin.Length, '*') : Resources.EnterPin; }
            set { _pin = value; }
        }

        public ICaptionCommand EnterValueCommand { get; set; }
        public ICaptionCommand SubmitPinCommand { get; set; }

        public LoginViewModel()
        {
            EnterValueCommand = new CaptionCommand<string>("Cmd", OnEnterValue);
            SubmitPinCommand = new CaptionCommand<string>("Gir", OnPinSubmitted);
        }

        private void OnPinSubmitted(string obj)
        {
            InvokePinSubmitted(_pin);
            _pin = "";
            RaisePropertyChanged(() => PinDisplay);
        }

        private void OnEnterValue(string obj)
        {
            if (!string.IsNullOrEmpty(obj))
                _pin += obj;
            else
                _pin = "";
            RaisePropertyChanged(() => PinDisplay);
        }
    }
}
