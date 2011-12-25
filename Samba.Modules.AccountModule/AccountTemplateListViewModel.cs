using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    [Export(typeof(AccountTemplateListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTemplateListViewModel : EntityCollectionViewModelBase<AccountTemplateViewModel, AccountTemplate>
    {
        private readonly IAccountService _accountService;

        [ImportingConstructor]
        public AccountTemplateListViewModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public AccountTemplateListViewModel()
        {
            
        }

        protected override string CanDeleteItem(AccountTemplate model)
        {
            if (_accountService.DidAccountTemplateUsed(model.Id))
                return Resources.DeleteErrorAccountTemplateAssignedtoAccounts;
            return base.CanDeleteItem(model);
        }
    }
}
