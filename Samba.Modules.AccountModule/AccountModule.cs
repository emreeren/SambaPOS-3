using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly AccountSelectorView _accountSelectorView;

        [ImportingConstructor]
        public AccountModule(IRegionManager regionManager, AccountSelectorView accountSelectorView)
            : base(regionManager, AppScreens.AccountList)
        {
            _regionManager = regionManager;
            _accountSelectorView = accountSelectorView;

            AddDashboardCommand<AccountListViewModel>(Resources.AccountList, Resources.Accounts, 40);
            AddDashboardCommand<AccountTemplateListViewModel>(Resources.AccountTemplateList, Resources.Accounts, 40);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeAccountTransaction, PermissionCategories.Cash, Resources.CanMakeAccountTransaction);
            PermissionRegistry.RegisterPermission(PermissionNames.CreditOrDeptAccount, PermissionCategories.Cash, Resources.CanMakeCreditOrDeptTransaction);
        }

        public override object GetVisibleView()
        {
            return _accountSelectorView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountSelectorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectAccount)
                {
                    Activate();
                    ((AccountSelectorViewModel)_accountSelectorView.DataContext).RefreshSelectedAccount();
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccountView) Activate();
                ((AccountSelectorViewModel)_accountSelectorView.DataContext).RefreshSelectedAccount();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccount)
                {
                    Activate();
                    ((AccountSelectorViewModel)_accountSelectorView.DataContext).DisplayAccount(x.Value);
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == EventTopicNames.SelectAccount)
                    {
                        Activate();
                        ((AccountSelectorViewModel)_accountSelectorView.DataContext).SearchAccount(x.Value.DataObject as string);
                    }
                }
                );
        }
    }
}
