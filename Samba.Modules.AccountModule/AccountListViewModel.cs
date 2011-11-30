using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    class AccountListViewModel : EntityCollectionViewModelBase<AccountViewModel, Account>
    {
        protected override AccountViewModel CreateNewViewModel(Account model)
        {
            return new AccountViewModel(model);
        }

        protected override Account CreateNewModel()
        {
            return new Account();
        }
    }
}
