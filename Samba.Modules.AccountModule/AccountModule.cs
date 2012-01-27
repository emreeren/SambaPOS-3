using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : ModuleBase
    {
        [ImportingConstructor]
        public AccountModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<AccountViewModel, Account>>(Resources.AccountList, Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTemplateViewModel, AccountTemplate>>(Resources.AccountTemplateList, Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionTemplateViewModel, AccountTransactionTemplate>>("Account Transaction Templates", Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionDocumentTemplateViewModel, AccountTransactionDocumentTemplate>>("Account Transaction Document Template", Resources.Accounts, 40);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeAccountTransaction, PermissionCategories.Cash, Resources.CanMakeAccountTransaction);
            PermissionRegistry.RegisterPermission(PermissionNames.CreditOrDeptAccount, PermissionCategories.Cash, Resources.CanMakeCreditOrDeptTransaction);
        }

        protected override void OnInitialization()
        {
        }
    }
}
