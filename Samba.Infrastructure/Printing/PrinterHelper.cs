using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Samba.Infrastructure.Printing
{
    public static class AsciiControlChars
    {
        /// <summary>
        /// Usually indicates the end of a string.
        /// </summary>
        public const char Nul = (char)0x00;

        /// <summary>
        /// Meant to be used for printers. When receiving this code the 
        /// printer moves to the next sheet of paper.
        /// </summary>
        public const char FormFeed = (char)0x0C;

        /// <summary>
        /// Starts an extended sequence of control codes.
        /// </summary>
        public const char Escape = (char)0x1B;

        /// <summary>
        /// Advances to the next line.
        /// </summary>
        public const char Newline = (char)0x0A;

        /// <summary>
        /// Defined to separate tables or different sets of data in a serial
        /// data storage system.
        /// </summary>
        public const char GroupSeparator = (char)0x1D;

        /// <summary>
        /// A horizontal tab.
        /// </summary>
        public const char HorizontalTab = (char)0x09;

        /// <summary>
        /// Returns the carriage to the start of the line.
        /// </summary>
        public const char CarriageReturn = (char)0x0D;

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        public const char Cancel = (char)0x18;

        /// <summary>
        /// Indicates that control characters present in the stream should
        /// be passed through as transmitted and not interpreted as control
        /// characters.
        /// </summary>
        public const char DataLinkEscape = (char)0x10;

        /// <summary>
        /// Signals the end of a transmission.
        /// </summary>
        public const char EndOfTransmission = (char)0x04;

        /// <summary>
        /// In serial storage, signals the separation of two files.
        /// </summary>
        public const char FileSeparator = (char)0x1C;
    }

    public class PrinterHelper
    {
        public static IntPtr GetPrinter(string szPrinterName)
        {
            var di = new DOCINFOA {pDocName = "Samba POS Document", pDataType = "RAW"};
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


