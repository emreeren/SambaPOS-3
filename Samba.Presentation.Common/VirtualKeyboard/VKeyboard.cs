using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Samba.Presentation.Common.VirtualKeyboard
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

            KeyA = new VKey("a", "A", Keys.A); VirtualKeys.Add(KeyA);
            KeyB = new VKey("b", "B", Keys.B); VirtualKeys.Add(KeyB);
            KeyC = new VKey("c", "C", Keys.C); VirtualKeys.Add(KeyC);
            KeyCTr = new VKey("ç", "Ç", Keys.Oem5); VirtualKeys.Add(KeyCTr);
            KeyD = new VKey("d", "D", Keys.D); VirtualKeys.Add(KeyD);
            KeyE = new VKey("e", "E", Keys.E); VirtualKeys.Add(KeyE);
            KeyF = new VKey("f", "F", Keys.F); VirtualKeys.Add(KeyF);
            KeyG = new VKey("g", "G", Keys.G); VirtualKeys.Add(KeyG);
            KeyGTr = new VKey("ğ", "Ğ", Keys.Oem4); VirtualKeys.Add(KeyGTr);
            KeyH = new VKey("h", "H", Keys.H); VirtualKeys.Add(KeyH);
            KeyI = new VKey("ı", "I", Keys.I); VirtualKeys.Add(KeyI);
            KeyITr = new VKey("i", "İ", Keys.Oem7); VirtualKeys.Add(KeyITr);
            KeyJ = new VKey("j", "J", Keys.J); VirtualKeys.Add(KeyJ);
            KeyK = new VKey("k", "K", Keys.K); VirtualKeys.Add(KeyK);
            KeyL = new VKey("l", "L", Keys.L); VirtualKeys.Add(KeyL);
            KeyM = new VKey("m", "M", Keys.M); VirtualKeys.Add(KeyM);
            KeyN = new VKey("n", "N", Keys.N); VirtualKeys.Add(KeyN);
            KeyO = new VKey("o", "O", Keys.O); VirtualKeys.Add(KeyO);
            KeyOTr = new VKey("ö", "Ö", Keys.Oem2); VirtualKeys.Add(KeyOTr);
            KeyP = new VKey("p", "P", Keys.P); VirtualKeys.Add(KeyP);
            KeyQ = new VKey("q", "Q", Keys.Q); VirtualKeys.Add(KeyQ);
            KeyR = new VKey("r", "R", Keys.R); VirtualKeys.Add(KeyR);
            KeyS = new VKey("s", "S", Keys.S); VirtualKeys.Add(KeyS);
            KeySTr = new VKey("ş", "Ş", Keys.Oem1); VirtualKeys.Add(KeySTr);
            KeyT = new VKey("t", "T", Keys.T); VirtualKeys.Add(KeyT);
            KeyU = new VKey("u", "U", Keys.U); VirtualKeys.Add(KeyU);
            KeyUTr = new VKey("ü", "Ü", Keys.Oem6); VirtualKeys.Add(KeyUTr);
            KeyV = new VKey("v", "V", Keys.V); VirtualKeys.Add(KeyV);
            KeyW = new VKey("w", "W", Keys.W); VirtualKeys.Add(KeyW);
            KeyX = new VKey("x", "X", Keys.X); VirtualKeys.Add(KeyX);
            KeyY = new VKey("y", "Y", Keys.Y); VirtualKeys.Add(KeyY);
            KeyZ = new VKey("z", "Z", Keys.Z); VirtualKeys.Add(KeyZ);
            Key1 = new VKey("1", "!", Keys.D1); VirtualKeys.Add(Key1);
            Key2 = new VKey("2", "'", Keys.D2); VirtualKeys.Add(Key2);
            Key3 = new VKey("3", "^", Keys.D3); VirtualKeys.Add(Key3);
            Key4 = new VKey("4", "+", Keys.D4); VirtualKeys.Add(Key4);
            Key5 = new VKey("5", "%", Keys.D5); VirtualKeys.Add(Key5);
            Key6 = new VKey("6", "&", Keys.D6); VirtualKeys.Add(Key6);
            Key7 = new VKey("7", "/", Keys.D7); VirtualKeys.Add(Key7);
            Key8 = new VKey("8", "(", Keys.D8); VirtualKeys.Add(Key8);
            Key9 = new VKey("9", ")", Keys.D9); VirtualKeys.Add(Key9);
            Key0 = new VKey("0", "=", Keys.D0); VirtualKeys.Add(Key0);
            KeyDoubleQuote = new VKey("\"", "é", Keys.Oem3); VirtualKeys.Add(KeyDoubleQuote);
            KeyTab = new VKey("Tab", "Tab", Keys.Tab); VirtualKeys.Add(KeyTab);
            KeyCaps = new VKey("Caps", "Caps", Keys.Capital); VirtualKeys.Add(KeyCaps);
            KeyShift = new VKey("Shift", "Shift", Keys.Shift); VirtualKeys.Add(KeyShift);
            KeyStar = new VKey("*", "?", Keys.Oem8); VirtualKeys.Add(KeyStar);
            KeyDash = new VKey("-", "_", Keys.OemMinus); VirtualKeys.Add(KeyDash);
            KeyBack = new VKey("BackSpace", "BackSpace", Keys.Back); VirtualKeys.Add(KeyBack);
            KeyEnter = new VKey("Enter", "Enter", Keys.Enter); VirtualKeys.Add(KeyEnter);
            KeyComma = new VKey(",", ";", Keys.Oemcomma); VirtualKeys.Add(KeyComma);
            KeyPoint = new VKey(".", ":", Keys.OemPeriod); VirtualKeys.Add(KeyPoint);
            KeyAt = new VKey("@", "€", Keys.Oem1); VirtualKeys.Add(KeyAt);
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
