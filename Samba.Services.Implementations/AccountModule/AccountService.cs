using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AccountModule
{
    [Export(typeof(IAccountService))]
    public class AccountService : AbstractService, IAccountService
    {
        [ImportingConstructor]
        public AccountService()
        {
            ValidatorRegistry.RegisterDeleteValidator(new AccountTemplateDeleteValidator());
        }

        private int? _accountCount;
        public int GetAccountCount()
        {
            return (int)(_accountCount ?? (_accountCount = Dao.Count<Account>()));
        }

        public override void Reset()
        {
            _accountCount = null;
        }
    }

    public class AccountTemplateDeleteValidator : SpecificationValidator<AccountTemplate>
    {
        public override string GetErrorMessage(AccountTemplate model)
        {
            if (Dao.Exists<Account>(x => x.AccountTemplate.Id == model.Id))
                return Resources.DeleteErrorAccountTemplateAssignedtoAccounts;
            return "";
        }
    }
}
