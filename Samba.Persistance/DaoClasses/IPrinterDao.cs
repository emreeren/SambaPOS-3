using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;

namespace Samba.Persistance.DaoClasses
{
    public interface IPrinterDao
    {
        IEnumerable<Printer> GetPrinters();
        IEnumerable<PrinterTemplate> GetPrinterTemplates();
        string GetMenuItemGroupCode(int menuItemId);
        string GetMenuItemData(int menuItemId, Expression<Func<MenuItem, string>> selector);
    }
}
