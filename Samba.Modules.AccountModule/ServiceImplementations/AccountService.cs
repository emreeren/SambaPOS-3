using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.AccountModule.ServiceImplementations
{
    [Export(typeof(IAccountService))]
    public class AccountService : AbstractService, IAccountService
    {
        private readonly int _accountCount;

        public AccountService()
        {
            _accountCount = Dao.Count<Account>(null);
        }

        public int GetAccountCount()
        {
            return _accountCount;
        }

        public override void Reset()
        {
            //;
        }
    }
}
