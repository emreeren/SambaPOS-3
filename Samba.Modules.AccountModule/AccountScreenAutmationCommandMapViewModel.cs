using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    public class AccountScreenAutmationCommandMapViewModel
    {
        private static string[] _commandValueTypes;
        public static string[] CommandValueTypes
        {
            get { return _commandValueTypes ?? (_commandValueTypes = new[] { Resources.Account, Resources.Entity }); }
        }

        private readonly AccountScreenAutmationCommandMap _model;
        private readonly ICacheService _cacheService;
        public AutomationCommand AutomationCommand { get; set; }

        public AccountScreenAutmationCommandMapViewModel(AccountScreenAutmationCommandMap model)
        {
            _model = model;
        }

        public AccountScreenAutmationCommandMapViewModel(AccountScreenAutmationCommandMap model, ICacheService cacheService)
            : this(model)
        {
            _cacheService = cacheService;
            AutomationCommand = _cacheService.GetAutomationCommandByName(model.AutomationCommandName);
        }

        public string AutomationCommandName { get { return _model.AutomationCommandName; } set { _model.AutomationCommandName = value; } }

        public string ButtonHeader { get { return AutomationCommand != null ? AutomationCommand.ButtonHeader : AutomationCommandName; } }

        public string AutomationCommandValueType
        {
            get { return CommandValueTypes[_model.AutomationCommandValueType]; }
            set { _model.AutomationCommandValueType = CommandValueTypes.ToList().IndexOf(value); }
        }
    }
}