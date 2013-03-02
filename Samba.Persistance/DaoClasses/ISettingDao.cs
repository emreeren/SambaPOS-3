using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Persistance.DaoClasses
{
    public interface ISettingDao
    {
        IEnumerable<TaxTemplate> GetTaxTemplates();
        IEnumerable<CalculationType> GetCalculationTypes();
        string GetNextString(int numeratorId);
        int GetNextNumber(int numeratorId);
        IEnumerable<Terminal> GetTerminals();
    }
}
