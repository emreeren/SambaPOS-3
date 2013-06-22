using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Samba.Presentation.Common;

namespace Samba.Presentation.Controls.VirtualKeyboard
{
    public enum KeyState
    {
        FirstSet,
        SecondSet
    }

    public class VKey : ObservableObject
    {
        private KeyState _keyState;
        public KeyState KeyState { get { return _keyState; } set { _keyState = value; RaisePropertyChanged(() => Caption); } }

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

        public VKey(Keys virtualKey)
        {
            VirtualKey = virtualKey;

            try
            {
                LowKey = User32Interop.ToUnicode(virtualKey, Keys.None);
            }
            catch (Exception) { LowKey = " "; }

            try
            {
                UpKey = User32Interop.ToUnicode(virtualKey, Keys.ShiftKey);
            }
            catch (Exception) { UpKey = " "; }
        }
    }

    public static class User32Interop
    {
        public static char ToAscii(Keys key, Keys modifiers)
        {
            var outputBuilder = new StringBuilder(2);
            int result = ToAscii((uint)key, 0, GetKeyState(modifiers),
                                 outputBuilder, 0);
            if (result == 1)
            {
                return outputBuilder[0];
            }

            ToAscii((uint)key, 0, GetKeyState(modifiers), outputBuilder, 0);
            return ' ';
        }

        public static string ToUnicode(Keys key, Keys modifiers)
        {
            var outputBuilder = new char[4];
            int result = ToUnicode((uint)key, 0, GetKeyState(modifiers),
                                 outputBuilder, 4, 0);
            if (result > 0)
            {
                return new string(outputBuilder.Take(result).ToArray());
            }

            ToUnicode((uint)key, 0, GetKeyState(modifiers), outputBuilder, 4, 0);
            return " ";
        }

        private const byte HighBit = 0x80;
        private static byte[] GetKeyState(Keys modifiers)
        {
            var keyState = new byte[256];
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if ((modifiers & key) == key)
                {
                    keyState[(int)key] = HighBit;
                }
            }
            return keyState;
        }

        [DllImport("user32.dll")]
        private static extern int ToAscii(uint uVirtKey, uint uScanCode,
                                          byte[] lpKeyState,
                                          [Out] StringBuilder lpChar,
                                          uint uFlags);

        //        int WINAPI ToUnicode(
        //  _In_      UINT wVirtKey,
        //  _In_      UINT wScanCode,
        //  _In_opt_  const BYTE *lpKeyState,
        //  _Out_     LPWSTR pwszBuff,
        //  _In_      int cchBuff,
        //  _In_      UINT wFlags
        //);
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(uint virtualKey, uint scanCode, byte[] keyStates, [MarshalAs(UnmanagedType.LPArray)] [Out] char[] chars, int charMaxCount, uint flags);
    }
}
