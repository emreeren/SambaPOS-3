using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    class AccountListViewModel : EntityCollectionViewModelBase<AccountEditorViewModel, Account>
    {
        protected override AccountEditorViewModel CreateNewViewModel(Account model)
        {
            return new AccountEditorViewModel(model);
        }

        protected override Account CreateNewModel()
        {
            return new Account();
        }
    }
}
