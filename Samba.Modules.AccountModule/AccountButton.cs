using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    public class AccountButton
    {
        private readonly AccountScreen _accountScreen;
        private readonly ICacheService _cacheService;

        public AccountButton(AccountScreen accountScreen, ICacheService cacheService)
        {
            _accountScreen = accountScreen;
            _cacheService = cacheService;
        }

        public string Caption { get { return _accountScreen.Name; } }
        public AccountScreen Model { get { return _accountScreen; } }
        public string ButtonColor { get { return "Gainsboro"; } }

        public IEnumerable<AccountType> AccountTypes { get { return _cacheService.GetAccountTypesByName(_accountScreen.AccountScreenValues.Select(x => x.AccountTypeName)); } }
    }
}