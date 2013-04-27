using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace Samba.Presentation.Common.Services
{
    public static class InteractionService
    {
        private const uint WmMousefirst = 0x200;
        private const uint WmMouselast = 0x209;

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PeekMessage(out NativeMessage message, IntPtr handle, uint filterMin, uint filterMax, uint flags);

        public static void ClearMouseClickQueue()
        {
            NativeMessage message;
            while (PeekMessage(out message, IntPtr.Zero, WmMousefirst, WmMouselast, 1))
            {
            }
        }

        public static IUserInteraction UserIntraction { get; set; }

        public static void ShowKeyboard()
        {
            UserIntraction.ShowKeyboard();
        }

        public static void HideKeyboard()
        {
            UserIntraction.HideKeyboard();
        }

        public static void ToggleKeyboard()
        {
            UserIntraction.ToggleKeyboard();
        }

        public static void Scale(FrameworkElement control)
        {
            UserIntraction.Scale(control);
        }
    }
}
