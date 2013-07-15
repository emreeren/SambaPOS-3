using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Common
{
    public interface ICustomPrinter
    {
        string Name { get; }
        object GetSettingsObject(string customPrinterData);
        void Process(Printer printer, string document);
    }
}
