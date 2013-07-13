using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Persistance;
using Samba.Services.Common;

namespace Samba.Services.Implementations.SettingsModule
{
    [Export(typeof(ISettingService))]
    public class SettingService : ISettingService
    {
        private readonly ISettingDao _settingDao;

        [ImportingConstructor]
        public SettingService(ISettingDao settingDao)
        {
            _settingDao = settingDao;
            _globalSettings = new ProgramSettings();
            _settingReplacer = new SettingReplacer(_globalSettings);
        }

        private readonly ProgramSettings _globalSettings;
        private readonly SettingReplacer _settingReplacer;

        private IEnumerable<Terminal> _terminals;
        public IEnumerable<Terminal> Terminals { get { return _terminals ?? (_terminals = _settingDao.GetTerminals()); } }

        public Terminal GetTerminalByName(string name)
        {
            return Terminals.SingleOrDefault(x => x.Name == name);
        }

        public Terminal GetDefaultTerminal()
        {
            return Terminals.SingleOrDefault(x => x.IsDefault);
        }

        public IEnumerable<Terminal> GetTerminals()
        {
            return Terminals;
        }

        public IProgramSettings ProgramSettings
        {
            get { return _globalSettings; }
        }

        public IProgramSetting GetProgramSetting(string settingName)
        {
            return _globalSettings.GetSetting(settingName);
        }

        public string ReplaceSettingValues(string value, string template)
        {
            return _settingReplacer.ReplaceSettingValue(template, value);
        }

        public void SaveProgramSettings()
        {
            _globalSettings.SaveChanges();
        }

        public IProgramSetting ReadLocalSetting(string settingName)
        {
            return _globalSettings.ReadLocalSetting(settingName);
        }

        public IProgramSetting ReadGlobalSetting(string settingName)
        {
            return _globalSettings.ReadGlobalSetting(settingName);
        }

        public IProgramSetting ReadSetting(string settingName)
        {
            return _globalSettings.ReadSetting(settingName);
        }

        public int GetNextNumber(int numeratorId)
        {
            return _settingDao.GetNextNumber(numeratorId);
        }

        public string GetNextString(int numeratorId)
        {
            return _settingDao.GetNextString(numeratorId);
        }

        public void ResetCache()
        {
            _terminals = null;
            _globalSettings.ResetCache();
            _settingReplacer.ClearCache();
        }

        public void ClearSettingCache()
        {
            _settingReplacer.ClearCache();
        }
    }
}
