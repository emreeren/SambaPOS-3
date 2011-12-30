using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    [Export(typeof(AccountTemplateListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTemplateListViewModel : EntityCollectionViewModelBase<AccountTemplateViewModel, AccountTemplate>
    {

    }
}
