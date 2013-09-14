using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Settings
{
    public class Printer : EntityClass
    {
        public string ShareName { get; set; }
        public int PrinterType { get; set; }
        public int CodePage { get; set; }
        public int CharsPerLine { get; set; }
        public int PageHeight { get; set; }
        public string CustomPrinterName { get; set; }
        public string CustomPrinterData { get; set; }

        public bool IsTicketPrinter { get { return PrinterType == 0; } }
        public bool IsTextPrinter { get { return PrinterType == 1; } }
        public bool IsHtmlPrinter { get { return PrinterType == 2; } }
        public bool IsPortPrinter { get { return PrinterType == 3; } }
        public bool IsDemoPrinter { get { return PrinterType == 4; } }
        public bool IsWindowsPrinter { get { return PrinterType == 5; } }
        public bool IsCustomPrinter { get { return PrinterType == 6; } }
        public bool IsRawPrinter { get { return PrinterType == 7; } }

        public Printer()
        {
            CharsPerLine = 42;
            CodePage = 857;
        }

        public void UpdateCustomSettings(object settingsObject)
        {
            CustomPrinterData = JsonHelper.Serialize(settingsObject);
        }
    }
}
