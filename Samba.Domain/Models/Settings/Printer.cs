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
        private string _customPrinterData;
        public string CustomPrinterData
        {
            get { return _customPrinterData; }
            set
            {
                _customPrinterData = value;
                //_customData = null;
            }
        }

        public bool IsTicketPrinter { get { return PrinterType == 0; } }
        public bool IsTextPrinter { get { return PrinterType == 1; } }
        public bool IsHtmlPrinter { get { return PrinterType == 2; } }
        public bool IsPortPrinter { get { return PrinterType == 3; } }
        public bool IsDemoPrinter { get { return PrinterType == 4; } }
        public bool IsWindowsPrinter { get { return PrinterType == 5; } }
        public bool IsCustomPrinter { get { return PrinterType == 6; } }

        public Printer()
        {
            CharsPerLine = 42;
            CodePage = 857;
        }

        //private Dictionary<string, string> _customData;
        //private string _customPrinterData;

        //public Dictionary<string, string> CustomData
        //{
        //    get { return _customData ?? (_customData = JsonHelper.Deserialize<Dictionary<string, string>>(CustomPrinterData)); }
        //}

        //public string GetCustomDataValue(string name)
        //{
        //    return CustomData.ContainsKey(name) ? CustomData[name] : "";
        //}

        //public void SetCustomData(string name, string value)
        //{
        //    if (!CustomData.ContainsKey(name))
        //        CustomData.Add(name, "");
        //    CustomData[name] = value;
        //    CustomPrinterData = JsonHelper.Serialize(CustomData);
        //}

        public void UpdateCustomSettings(object settingsObject)
        {
            CustomPrinterData = JsonHelper.Serialize(settingsObject);
        }
    }
}
