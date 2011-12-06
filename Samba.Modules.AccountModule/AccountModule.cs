using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : ModuleBase
    {
        [ImportingConstructor]
        public AccountModule()
        {
            AddDashboardCommand<AccountListViewModel>(Resources.AccountList, Resources.Accounts, 40);
            AddDashboardCommand<AccountTemplateListViewModel>(Resources.AccountTemplateList, Resources.Accounts, 40);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeAccountTransaction, PermissionCategories.Cash, Resources.CanMakeAccountTransaction);
            PermissionRegistry.RegisterPermission(PermissionNames.CreditOrDeptAccount, PermissionCategories.Cash, Resources.CanMakeCreditOrDeptTransaction);
        }

        protected override void OnInitialization()
        {
        }
    }
}
