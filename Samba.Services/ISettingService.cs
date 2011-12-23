using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;

namespace Samba.Services
{
    public interface ISettingService
    {
        ServiceTemplate GetServiceTemplateById(int id);
        ServiceTemplate GetServiceTemplateByName(string name);
        TaxTemplate GetTaxTemplateById(int id);
        TaxTemplate GetTaxTemplateByName(string name);
        
    }
}
