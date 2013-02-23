using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Samba.Presentation.Common;

namespace Samba.Presentation.Controls.VirtualKeyboard
{
    public class VKeyboard
    {
        public VKey KeyA { get; set; }
        public VKey KeyB { get; set; }
        public VKey KeyC { get; set; }
        public VKey KeyCTr { get; set; }
        public VKey KeyD { get; set; }
        public VKey KeyE { get; set; }
        public VKey KeyF { get; set; }
        public VKey KeyG { get; set; }
        public VKey KeyGTr { get; set; }
        public VKey KeyH { get; set; }
        public VKey KeyI { get; set; }
        public VKey KeyITr { get; set; }
        public VKey KeyJ { get; set; }
        public VKey KeyK { get; set; }
        public VKey KeyL { get; set; }
        public VKey KeyM { get; set; }
        public VKey KeyN { get; set; }
        public VKey KeyO { get; set; }
        public VKey KeyOTr { get; set; }
        public VKey KeyP { get; set; }
        public VKey KeyQ { get; set; }
        public VKey KeyR { get; set; }
        public VKey KeyS { get; set; }
        public VKey KeySTr { get; set; }
        public VKey KeyT { get; set; }
        public VKey KeyU { get; set; }
        public VKey KeyUTr { get; set; }
        public VKey KeyV { get; set; }
        public VKey KeyW { get; set; }
        public VKey KeyX { get; set; }
        public VKey KeyY { get; set; }
        public VKey KeyZ { get; set; }
        public VKey Key1 { get; set; }
        public VKey Key2 { get; set; }
        public VKey Key3 { get; set; }
        public VKey Key4 { get; set; }
        public VKey Key5 { get; set; }
        public VKey Key6 { get; set; }
        public VKey Key7 { get; set; }
        public VKey Key8 { get; set; }
        public VKey Key9 { get; set; }
        public VKey Key0 { get; set; }
        public VKey KeyDoubleQuote { get; set; }
        public VKey KeyTab { get; set; }
        public VKey KeyCaps { get; set; }
        public VKey KeyShift { get; set; }
        public VKey KeyStar { get; set; }
        public VKey KeyDash { get; set; }
        public VKey KeyBack { get; set; }
        public VKey KeyEnter { get; set; }
        public VKey KeyComma { get; set; }
        public VKey KeyPoint { get; set; }
        public VKey KeyAt { get; set; }
        public VKey KeySpace { get; set; }
        public VKey UpArrow { get; set; }
        public VKey DownArrow { get; set; }

        public IList<VKey> VirtualKeys { get; set; }

        public VKeyboard()
        {
            VirtualKeys = new List<VKey>();

            KeyA = new VKey(Keys.A); VirtualKeys.Add(KeyA);
            KeyB = new VKey(Keys.B); VirtualKeys.Add(KeyB);
            KeyC = new VKey(Keys.C); VirtualKeys.Add(KeyC);
            KeyCTr = new VKey(Keys.Oem5); VirtualKeys.Add(KeyCTr);
            KeyD = new VKey(Keys.D); VirtualKeys.Add(KeyD);
            KeyE = new VKey(Keys.E); VirtualKeys.Add(KeyE);
            KeyF = new VKey(Keys.F); VirtualKeys.Add(KeyF);
            KeyG = new VKey(Keys.G); VirtualKeys.Add(KeyG);
            KeyGTr = new VKey(Keys.Oem4); VirtualKeys.Add(KeyGTr);
            KeyH = new VKey(Keys.H); VirtualKeys.Add(KeyH);
            KeyI = new VKey(Keys.I); VirtualKeys.Add(KeyI);
            KeyITr = new VKey(Keys.Oem7); VirtualKeys.Add(KeyITr);
            KeyJ = new VKey(Keys.J); VirtualKeys.Add(KeyJ);
            KeyK = new VKey(Keys.K); VirtualKeys.Add(KeyK);
            KeyL = new VKey(Keys.L); VirtualKeys.Add(KeyL);
            KeyM = new VKey(Keys.M); VirtualKeys.Add(KeyM);
            KeyN = new VKey(Keys.N); VirtualKeys.Add(KeyN);
            KeyO = new VKey(Keys.O); VirtualKeys.Add(KeyO);
            KeyOTr = new VKey(Keys.Oem2); VirtualKeys.Add(KeyOTr);
            KeyP = new VKey(Keys.P); VirtualKeys.Add(KeyP);
            KeyQ = new VKey(Keys.Q); VirtualKeys.Add(KeyQ);
            KeyR = new VKey(Keys.R); VirtualKeys.Add(KeyR);
            KeyS = new VKey(Keys.S); VirtualKeys.Add(KeyS);
            KeySTr = new VKey(Keys.Oem1); VirtualKeys.Add(KeySTr);
            KeyT = new VKey(Keys.T); VirtualKeys.Add(KeyT);
            KeyU = new VKey(Keys.U); VirtualKeys.Add(KeyU);
            KeyUTr = new VKey(Keys.Oem6); VirtualKeys.Add(KeyUTr);
            KeyV = new VKey(Keys.V); VirtualKeys.Add(KeyV);
            KeyW = new VKey(Keys.W); VirtualKeys.Add(KeyW);
            KeyX = new VKey(Keys.X); VirtualKeys.Add(KeyX);
            KeyY = new VKey(Keys.Y); VirtualKeys.Add(KeyY);
            KeyZ = new VKey(Keys.Z); VirtualKeys.Add(KeyZ);
            Key1 = new VKey(Keys.D1); VirtualKeys.Add(Key1);
            Key2 = new VKey(Keys.D2); VirtualKeys.Add(Key2);
            Key3 = new VKey(Keys.D3); VirtualKeys.Add(Key3);
            Key4 = new VKey(Keys.D4); VirtualKeys.Add(Key4);
            Key5 = new VKey(Keys.D5); VirtualKeys.Add(Key5);
            Key6 = new VKey(Keys.D6); VirtualKeys.Add(Key6);
            Key7 = new VKey(Keys.D7); VirtualKeys.Add(Key7);
            Key8 = new VKey(Keys.D8); VirtualKeys.Add(Key8);
            Key9 = new VKey(Keys.D9); VirtualKeys.Add(Key9);
            Key0 = new VKey(Keys.D0); VirtualKeys.Add(Key0);
            KeyDoubleQuote = new VKey(Keys.Oem3); VirtualKeys.Add(KeyDoubleQuote);
            KeyTab = new VKey("Tab", "Tab", Keys.Tab); VirtualKeys.Add(KeyTab);
            KeyCaps = new VKey("Caps", "Caps", Keys.Capital); VirtualKeys.Add(KeyCaps);
            KeyShift = new VKey("Shift", "Shift", Keys.Shift); VirtualKeys.Add(KeyShift);
            KeyStar = new VKey(Keys.Oem8); VirtualKeys.Add(KeyStar);
            KeyDash = new VKey(Keys.OemMinus); VirtualKeys.Add(KeyDash);
            KeyBack = new VKey("BackSpace", "BackSpace", Keys.Back); VirtualKeys.Add(KeyBack);
            KeyEnter = new VKey("Enter", "Enter", Keys.Enter); VirtualKeys.Add(KeyEnter);
            KeyComma = new VKey(Keys.Oemcomma); VirtualKeys.Add(KeyComma);
            KeyPoint = new VKey(Keys.OemPeriod); VirtualKeys.Add(KeyPoint);
            KeyAt = new VKey("@", "€", Keys.Oem102); VirtualKeys.Add(KeyAt);
            KeySpace = new VKey(" ", "Space", Keys.Space); VirtualKeys.Add(KeySpace);
            UpArrow = new VKey("Up", "Up", Keys.Up); VirtualKeys.Add(UpArrow);
            DownArrow = new VKey("Down", "Down", Keys.Down); VirtualKeys.Add(DownArrow);
        }

        public void PressKey(Keys keyCode)
        {
            var structInput = new INPUT();
            structInput.type = 1;
            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = 0;
            // Key down the actual key-code

            structInput.ki.wVk = (ushort)keyCode; //VK.SHIFT etc.
            NativeWin32.SendInput(1, ref structInput, Marshal.SizeOf(structInput));
        }

        public void ReleaseKey(Keys keyCode)
        {
            var structInput = new INPUT();
            structInput.type = 1;
            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = 0;

            // Key up the actual key-code
            structInput.ki.dwFlags = NativeWin32.KEYEVENTF_KEYUP;
            structInput.ki.wVk = (ushort)keyCode;// (ushort)NativeWin32.VK.SNAPSHOT;//vk;
            NativeWin32.SendInput(1, ref structInput, Marshal.SizeOf(structInput));
        }

        public void ProcessKey(Keys keyCode)
        {
            if (keyCode == Keys.Shift)
            {
                ToggleShift();
            }
            else
            {
                SendKey(keyCode);
                if (_shifted) ToggleShift();
            }

        }

        private bool _shifted;

        private void ToggleShift()
        {
            _shifted = !_shifted;
            foreach (var virtualKey in VirtualKeys)
            {
                virtualKey.KeyState = _shifted ? KeyState.SecondSet : KeyState.FirstSet;
            }
            if (_shifted) PressKey(Keys.LShiftKey); else ReleaseKey(Keys.LShiftKey);
        }

        public void SendKey(Keys keyCode)
        {
            PressKey(keyCode);
            ReleaseKey(keyCode);
        }
    }
}
