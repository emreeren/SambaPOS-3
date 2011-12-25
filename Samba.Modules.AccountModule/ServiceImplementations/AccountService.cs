using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
using Samba.Persistance.Data;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.AccountModule.ServiceImplementations
{
    [Export(typeof(IAccountService))]
    public class AccountService : AbstractService, IAccountService
    {
        private int? _accountCount;

        public int GetAccountCount()
        {
            return (int)(_accountCount ?? (_accountCount = Dao.Count<Account>(null)));
        }

        public override void Reset()
        {
            _accountCount = null;
        }
    }
}
