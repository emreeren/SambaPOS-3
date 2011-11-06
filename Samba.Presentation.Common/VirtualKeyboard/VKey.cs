using System.Windows.Forms;

namespace Samba.Presentation.Common.VirtualKeyboard
{
    public enum KeyState
    {
        FirstSet,
        SecondSet
    }

    public class VKey : ObservableObject
    {
        private KeyState _keyState;
        public KeyState KeyState
        {
            get { return _keyState; }
            set
            {
                _keyState = value;
                RaisePropertyChanged(() => Caption);
            }
        }

        public string Caption
        {
            get { return KeyState == KeyState.FirstSet ? LowKey : UpKey; }
        }

        public string LowKey { get; set; }
        public string UpKey { get; set; }
        public Keys VirtualKey { get; set; }

        public VKey(string lowKey, string upKey, Keys virtualKey)
        {
            LowKey = lowKey;
            UpKey = upKey;
            VirtualKey = virtualKey;
        }
    }
}
