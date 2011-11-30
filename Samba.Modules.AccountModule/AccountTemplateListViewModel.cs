using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;


namespace Samba.Modules.AccountModule
{
    class AccountTemplateListViewModel : EntityCollectionViewModelBase<AccountTemplateViewModel, AccountTemplate>
    {
        protected override AccountTemplateViewModel CreateNewViewModel(AccountTemplate model)
        {
            return new AccountTemplateViewModel(model);
        }

        protected override AccountTemplate CreateNewModel()
        {
            return new AccountTemplate();
        }

        protected override string CanDeleteItem(AccountTemplate model)
        {
            if (Dao.Count<Account>(x => x.AccountTemplate.Id == model.Id) > 0)
                return Resources.DeleteErrorAccountTemplateAssignedtoAccounts;
            return base.CanDeleteItem(model);
        }
    }
}
