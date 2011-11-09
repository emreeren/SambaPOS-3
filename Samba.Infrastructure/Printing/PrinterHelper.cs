using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Samba.Infrastructure.Printing
{
    public static class AsciiControlChars
    {
        public const char Nul = (char)0x00;
        public const char FormFeed = (char)0x0C;
        public const char Escape = (char)0x1B;
        public const char Newline = (char)0x0A;
        public const char GroupSeparator = (char)0x1D;
        public const char HorizontalTab = (char)0x09;
        public const char CarriageReturn = (char)0x0D;
        public const char Cancel = (char)0x18;
        public const char DataLinkEscape = (char)0x10;
        public const char EndOfTransmission = (char)0x04;
        public const char FileSeparator = (char)0x1C;
    }

    public class PrinterHelper
    {
        public static IntPtr GetPrinter(string szPrinterName)
        {
            var di = new NativeMethods.DOCINFOA { pDocName = "Samba POS Document", pDataType = "RAW" };
            IntPtr hPrinter;
            if (!NativeMethods.OpenPrinter(szPrinterName, out hPrinter, IntPtr.Zero)) BombWin32();
            if (!NativeMethods.StartDocPrinter(hPrinter, 1, di)) BombWin32();
            if (!NativeMethods.StartPagePrinter(hPrinter)) BombWin32();
            return hPrinter;
        }

        public static void EndPrinter(IntPtr hPrinter)
        {
            NativeMethods.EndPagePrinter(hPrinter);
            NativeMethods.EndDocPrinter(hPrinter);
            NativeMethods.ClosePrinter(hPrinter);
        }

        public static void SendBytesToPrinter(string szPrinterName, byte[] pBytes)
        {
            var hPrinter = GetPrinter(szPrinterName);
            int dwWritten;
            if (!NativeMethods.WritePrinter(hPrinter, pBytes, pBytes.Length, out dwWritten)) BombWin32();
            EndPrinter(hPrinter);
        }

        public static void SendFileToPrinter(string szPrinterName, string szFileName)
        {
            using (var fs = new FileStream(szFileName, FileMode.Open))
            {
                var len = (int)fs.Length;
                var bytes = new Byte[len];
                fs.Read(bytes, 0, len);
                SendBytesToPrinter(szPrinterName, bytes);
            }
        }

        private static void BombWin32()
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);
        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool ClosePrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, DOCINFOA di);
        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool EndDocPrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool StartPagePrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool EndPagePrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool WritePrinter(IntPtr hPrinter, byte[] pBytes, Int32 dwCount, out Int32 dwWritten);
    }
}


