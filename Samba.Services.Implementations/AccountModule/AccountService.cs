using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AccountModule
{
    [Export(typeof(IAccountService))]
    public class AccountService : AbstractService, IAccountService
    {
        private int? _accountCount;

        public int GetAccountCount()
        {
            return (int)(_accountCount ?? (_accountCount = Dao.Count<Account>()));
        }

        public bool DidAccountTemplateUsed(int accountTemplateId)
        {
            return (Dao.Count<Account>(x => x.AccountTemplate.Id == accountTemplateId) > 0);
        }

        public override void Reset()
        {
            _accountCount = null;
        }
    }
}
