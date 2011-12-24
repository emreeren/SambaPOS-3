using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;

namespace Samba.Services
{
    public interface ISettingService
    {
        ServiceTemplate GetServiceTemplateById(int id);
        ServiceTemplate GetServiceTemplateByName(string name);
        TaxTemplate GetTaxTemplateById(int id);
        TaxTemplate GetTaxTemplateByName(string name);
        Terminal GetTerminalByName(string name);
        Terminal GetDefaultTerminal();
        IEnumerable<string> GetTerminalNames();
    }
}
