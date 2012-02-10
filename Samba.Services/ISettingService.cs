using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ISettingService
    {
        CalculationTemplate GetCalculationTemplateById(int id);
        CalculationTemplate GetCalculationTemplateByName(string name);
        TaxTemplate GetTaxTemplateById(int id);
        TaxTemplate GetTaxTemplateByName(string name);
        Terminal GetTerminalByName(string name);
        Terminal GetDefaultTerminal();
        IEnumerable<string> GetTerminalNames();
        IProgramSettings ProgramSettings { get; }
        IProgramSetting GetProgramSetting(string settingName);
        IProgramSetting ReadSetting(string settingName);
        IProgramSetting ReadLocalSetting(string settingName);
        IProgramSetting ReadGlobalSetting(string settingName);
        ISettingReplacer GetSettingReplacer();
        void SaveProgramSettings();
        int GetNextNumber(int numeratorId);
        string GetNextString(int numeratorId);

    }
}
