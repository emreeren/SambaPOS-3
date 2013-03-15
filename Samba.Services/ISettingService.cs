using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface ISettingService
    {
        CalculationType GetCalculationTypeById(int id);
        CalculationType GetCalculationTypeByName(string name);
        TaxTemplate GetTaxTemplateById(int id);
        TaxTemplate GetTaxTemplateByName(string name);
        Terminal GetTerminalByName(string name);
        Terminal GetDefaultTerminal();
        IEnumerable<string> GetTerminalNames();
        IEnumerable<Terminal> GetTerminals();
        IProgramSettings ProgramSettings { get; }
        IProgramSetting GetProgramSetting(string settingName);
        IProgramSetting ReadSetting(string settingName);
        IProgramSetting ReadLocalSetting(string settingName);
        IProgramSetting ReadGlobalSetting(string settingName);
        string ReplaceSettingValues(string value, string template = "\\{:([^}]+)\\}"); 
        void SaveProgramSettings();
        int GetNextNumber(int numeratorId);
        string GetNextString(int numeratorId);
        void ResetCache();
        void ClearSettingCache();
    }
}
