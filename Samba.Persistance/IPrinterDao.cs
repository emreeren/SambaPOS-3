using System.Collections.Generic;
using Samba.Domain.Models.Settings;

namespace Samba.Persistance
{
    public interface IPrinterDao
    {
        IEnumerable<Printer> GetPrinters();
        IEnumerable<PrinterTemplate> GetPrinterTemplates();
    }
}
