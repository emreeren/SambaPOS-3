using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Modules.AccountModule.Dashboard;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly AccountDetailsView _accountDetailsView;
        private readonly DocumentCreatorView _documentCreatorView;

        [ImportingConstructor]
        public AccountModule(IRegionManager regionManager, 
            AccountDetailsView accountDetailsView,
            DocumentCreatorView documentCreatorView)
        {
            _regionManager = regionManager;
            _accountDetailsView = accountDetailsView;
            _documentCreatorView = documentCreatorView;

            AddDashboardCommand<EntityCollectionViewModelBase<AccountTemplateViewModel, AccountTemplate>>(string.Format(Resources.List_f, Resources.AccountTemplate), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountViewModel, Account>>(string.Format(Resources.List_f, Resources.Account), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionTemplateViewModel, AccountTransactionTemplate>>(string.Format(Resources.List_f, "Transaction Template"), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionDocumentViewModel, AccountTransactionDocument>>(string.Format(Resources.List_f, "Transaction Document"), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionDocumentTemplateViewModel, AccountTransactionDocumentTemplate>>(string.Format(Resources.List_f, "Document Template"), Resources.Accounts, 40);

            PermissionRegistry.RegisterPermission(PermissionNames.MakeAccountTransaction, PermissionCategories.Cash, Resources.CanMakeAccountTransaction);
            PermissionRegistry.RegisterPermission(PermissionNames.CreditOrDeptAccount, PermissionCategories.Cash, Resources.CanMakeCreditOrDeptTransaction);
            
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountDetailsView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(DocumentCreatorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<DocumentCreationData>>().Subscribe(x =>
                {
                    if (x.Topic == EventTopicNames.AccountTransactionDocumentSelected)
                    {
                        ActivateDocumentCreator();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Account>>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.DisplayAccountTransactions)
                    {
                        ActivateAccountTransactions();
                    }
                });
        }

        private void ActivateDocumentCreator()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_documentCreatorView);
        }
        
        private void ActivateAccountTransactions()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountDetailsView);
        }

    }
}
