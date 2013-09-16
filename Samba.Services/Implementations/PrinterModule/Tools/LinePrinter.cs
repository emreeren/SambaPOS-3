using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Zen.Barcode;

namespace Samba.Services.Implementations.PrinterModule.Tools
{
    public enum LineAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    internal class BitmapData
    {
        public BitArray Dots
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }
    }

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

    public class LinePrinter
    {
        private readonly string _printerName;
        private IntPtr _hprinter = IntPtr.Zero;
        private readonly int _maxChars;
        private readonly int _codePage;

        public LinePrinter(string printerName, int maxChars, int codepage)
        {
            _maxChars = maxChars;
            _codePage = codepage;
            _printerName = printerName;
        }

        public void Beep(char times = '\x2', char duration = '\x5')
        {
            WriteData(AsciiControlChars.Escape + "B" + times + duration);
        }

        public void EnableCenter()
        {
            WriteData(AsciiControlChars.Escape + "a" + (char)1);
        }

        public void EnableLeft()
        {
            WriteData(AsciiControlChars.Escape + "a" + (char)0);
        }

        public void EnableRight()
        {
            WriteData(AsciiControlChars.Escape + "a" + (char)2);
        }

        public void EnableBold()
        {
            WriteData(AsciiControlChars.Escape + "G" + (char)1);
        }

        public void DisableBold()
        {
            WriteData(AsciiControlChars.Escape + "G" + (char)0);
        }

        public void SelectTurkishCodePage()
        {
            WriteData(AsciiControlChars.Escape + (char)0x1D + "t" + (char)12);
        }

        public void Cut()
        {
            WriteData(AsciiControlChars.Escape + "d" + (char)1);
            WriteData(AsciiControlChars.GroupSeparator + "V" + (char)66 + (char)0);
        }

        public void WriteLine(string line)
        {
            WriteLine(line, 0, 0, LineAlignment.Left);
        }

        public void WriteLine(string line, int height, int width, LineAlignment alignment)
        {
            int h = height + (width * 16);
            WriteData(AsciiControlChars.GroupSeparator + "!" + (char)h);
            WriteData(line + (char)0xA);
        }

        public void PrintWindow(string line)
        {
            const string tl = "┌";
            const string tr = "┐";
            const string bl = "└";
            const string br = "┘";
            const string vl = "│";
            const string hl = "─";
            const string s = "░";

            WriteLine(tl + hl.PadLeft(_maxChars - 2, hl[0]) + tr, 1, 0, LineAlignment.Left);
            string text = vl + line.PadLeft((((_maxChars - 2) + line.Length) / 2), s[0]);
            WriteLine(text + vl.PadLeft(_maxChars - text.Length, s[0]), 1, 0, LineAlignment.Left);
            WriteLine(bl + hl.PadLeft(_maxChars - 2, hl[0]) + br);
        }

        public void PrintFullLine(char lineChar)
        {
            WriteLine(lineChar.ToString(CultureInfo.InvariantCulture).PadLeft(_maxChars, lineChar));
        }

        public void PrintCenteredLabel(string label, bool expandLabel)
        {
            if (expandLabel) label = ExpandLabel(label);
            string text = label.PadLeft((((_maxChars) + label.Length) / 2), '░');
            WriteLine(text + "░".PadLeft(_maxChars - text.Length, '░'), 1, 0, LineAlignment.Left);
        }

        private static string ExpandLabel(string label)
        {
            string result = "";
            for (int i = 0; i < label.Length - 1; i++)
            {
                result += label[i] + " ";
            }
            result += label[label.Length - 1];
            return result;
        }

        public void StartDocument()
        {
            if (_hprinter == IntPtr.Zero)
                _hprinter = PrinterHelper.GetPrinter(_printerName);
        }

        public void WriteData(byte[] data)
        {
            if (_hprinter != IntPtr.Zero)
            {
                int dwWritten;
                if (!PrinterHelper.WritePrinter(_hprinter, data, data.Length, out dwWritten)) BombWin32();
            }
        }

        public void WriteData(string data)
        {
            byte[] pBytes = Encoding.GetEncoding(_codePage).GetBytes(data);
            WriteData(pBytes);
        }

        public void EndDocument()
        {
            PrinterHelper.EndPrinter(_hprinter);
            _hprinter = IntPtr.Zero;
        }

        private static void BombWin32()
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        public void PrintBitmap(string fileName)
        {
            if (File.Exists(fileName))
            {
                var space = GetSpace(fileName);
                fileName = fileName.TrimStart();
                PrintBitmap((Bitmap)Image.FromFile(fileName), space);
            }
        }

        public void PrintBitmap(Bitmap bitmap, int space)
        {
            byte[] data = GetDocument(bitmap, space);
            WriteData(data);
        }

        public void PrintQrCode(string qrCodeData, int size, int ec)
        {
            if (string.IsNullOrWhiteSpace(qrCodeData)) return;

            var space = GetSpace(qrCodeData);
            qrCodeData = qrCodeData.TrimStart();
            
            if (size == 0) size = 6;
            else size = (size * 2) + 1;

            if (ec > 0) ec--;

            var qrEncoder = new QrEncoder((ErrorCorrectionLevel)ec);
            var qrCode = qrEncoder.Encode(qrCodeData);

            var renderer = new GraphicsRenderer(new FixedModuleSize(size, QuietZoneModules.Zero), Brushes.Black, Brushes.White);
            using (var stream = new MemoryStream())
            {
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Bmp, stream);
                PrintBitmap((Bitmap)Image.FromStream(stream), space);
            }
        }

        private int GetSpace(string qrCodeData)
        {
            var result = 0;
            if (qrCodeData.StartsWith(""))
            {
                while (result < qrCodeData.Length && qrCodeData[result] == ' ')
                {
                    result++;
                }
            }
            return result;
        }

        public void PrintBarCode(string barcode, int start, int end)
        {
            var bcType = start.ToString(CultureInfo.InvariantCulture) + end.ToString(CultureInfo.InvariantCulture);
            BarcodeDraw barcodeDraw = BarcodeDrawFactory.Code128WithChecksum;
            if (bcType == "39") barcodeDraw = BarcodeDrawFactory.Code39WithoutChecksum;
            if (bcType == "13") barcodeDraw = BarcodeDrawFactory.CodeEan13WithChecksum;
            if (bcType == "25") barcodeDraw = BarcodeDrawFactory.Code25StandardWithoutChecksum;
            if (bcType == "08") barcodeDraw = BarcodeDrawFactory.CodeEan8WithChecksum;
            if (bcType == "11") barcodeDraw = BarcodeDrawFactory.Code11WithoutChecksum;
            if (bcType == "93") barcodeDraw = BarcodeDrawFactory.Code93WithChecksum;

            var space = GetSpace(barcode);
            barcode = barcode.TrimStart();
            using (var img = barcodeDraw.Draw(barcode, 80, 2))
            {
                PrintBitmap(new Bitmap(img), space);
            }
        }

        private static BitmapData ConvertBitmapData(Bitmap bitmap)
        {
            const int threshold = 127;
            var index = 0;
            var dimensions = bitmap.Width * bitmap.Height;
            var dots = new BitArray(dimensions);

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    dots[index] = (luminance < threshold);
                    index++;
                }
            }

            return new BitmapData
                       {
                           Dots = dots,
                           Height = bitmap.Height,
                           Width = bitmap.Width
                       };
        }

        private static void RenderLogo(BinaryWriter bw, Bitmap bitmap, int space)
        {
            var data = ConvertBitmapData(bitmap);
            RenderBitmap(bw, data, space);
        }

        private static void RenderBitmap(BinaryWriter bw, BitmapData data, int space)
        {
            var dots = data.Dots;
            var width = BitConverter.GetBytes(data.Width);
            
            bw.Write(AsciiControlChars.Escape);
            bw.Write('3');
            bw.Write((byte)24);

            int offset = 0;

            while (offset < data.Height)
            {
                bw.Write("".PadLeft(space));
                
                bw.Write(AsciiControlChars.Escape);
                bw.Write('*'); // bit-image mode
                bw.Write((byte)33); // 24-dot double-density
                bw.Write(width[0]); // width low byte
                bw.Write(width[1]); // width high byte

                for (var x = 0; x < data.Width; ++x)
                {
                    for (var k = 0; k < 3; ++k)
                    {
                        byte slice = 0;

                        for (var b = 0; b < 8; ++b)
                        {
                            var y = (((offset / 8) + k) * 8) + b;
                            var i = (y * data.Width) + x;
                            var v = false;

                            if (i < dots.Length)
                                v = dots[i];

                            slice |= (byte)((v ? 1 : 0) << (7 - b));
                        }
                        bw.Write(slice);
                    }
                }
                offset += 24;
                bw.Write(AsciiControlChars.Newline);
            }

            bw.Write(AsciiControlChars.Escape);
            bw.Write('3');
            bw.Write((byte)30);
        }

        private static byte[] GetDocument(Bitmap bitmap, int space)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                using (var bw = new BinaryWriter(ms))
                {
                    ms = null;
                    //bw.Write(AsciiControlChars.Escape);
                    //bw.Write('@');
                    RenderLogo(bw, bitmap, space);
                    bw.Flush();
                    return ((MemoryStream)bw.BaseStream).ToArray();
                }
            }
            finally
            {
                if (ms != null)
                    ms.Dispose();
            }
        }

        public void OpenCashDrawer()
        {
            // http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/35575dd8-7593-4fe6-9b57-64ad6b5f7ae6/
            WriteData(((char)27 + (char)112 + (char)0 + (char)25 + (char)250).ToString(CultureInfo.InvariantCulture));
        }

        public void ExecCommand(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                var data = command.Trim().Split(',').Select(x => Convert.ToInt32(x)).Aggregate("", (current, i) => current + (char)i);
                WriteData(data);
            }
        }

        public static void SendBytesToPrinter(string szPrinterName, byte[] pBytes)
        {
            var hPrinter = PrinterHelper.GetPrinter(szPrinterName);
            int dwWritten;
            if (!PrinterHelper.WritePrinter(hPrinter, pBytes, pBytes.Length, out dwWritten)) BombWin32();
            PrinterHelper.EndPrinter(hPrinter);
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
    }
}


