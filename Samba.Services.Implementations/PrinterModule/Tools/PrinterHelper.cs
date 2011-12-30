using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Samba.Services.Implementations.PrinterModule.Tools
{
    public class PrinterHelper
    {
        public static IntPtr GetPrinter(string szPrinterName)
        {
            var di = new DOCINFOA { pDocName = "Samba POS Document", pDataType = "RAW" };
            IntPtr hPrinter;
            if (!OpenPrinter(szPrinterName, out hPrinter, IntPtr.Zero)) BombWin32();
            if (!StartDocPrinter(hPrinter, 1, di)) BombWin32();
            if (!StartPagePrinter(hPrinter)) BombWin32();
            return hPrinter;
        }

        public static void EndPrinter(IntPtr hPrinter)
        {
            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
            ClosePrinter(hPrinter);
        }

        public static void SendBytesToPrinter(string szPrinterName, byte[] pBytes)
        {
            var hPrinter = GetPrinter(szPrinterName);
            int dwWritten;
            if (!WritePrinter(hPrinter, pBytes, pBytes.Length, out dwWritten)) BombWin32();
            EndPrinter(hPrinter);
        }

        public static void SendFileToPrinter(string szPrinterName, string szFileName)
        {
            var fs = new FileStream(szFileName, FileMode.Open);
            var len = (int)fs.Length;
            var bytes = new Byte[len];
            fs.Read(bytes, 0, len);
            SendBytesToPrinter(szPrinterName, bytes);
        }

        private static void BombWin32()
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DOCINFOA
        {
            public string pDocName;
            public string pOutputFile;
            public string pDataType;
        }

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);
        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, DOCINFOA di);
        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);
        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, byte[] pBytes, Int32 dwCount, out Int32 dwWritten);

    }
}


