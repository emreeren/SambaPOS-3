using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Modules.AccountModule.Dashboard;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;
        private readonly AccountSelectorView _accountSelectorView;
        private readonly AccountSelectorViewModel _accountSelectorViewModel;
        private readonly AccountDetailsView _accountDetailsView;
        private readonly DocumentCreatorView _documentCreatorView;
        private readonly BatchDocumentCreatorView _batchDocumentCreatorView;
        private readonly BatchDocumentCreatorViewModel _batchDocumentCreatorViewModel;

        [ImportingConstructor]
        public AccountModule(IRegionManager regionManager,
            IUserService userService, IApplicationState applicationState,
            AccountSelectorView accountSelectorView, AccountSelectorViewModel accountSelectorViewModel,
            AccountDetailsView accountDetailsView,
            DocumentCreatorView documentCreatorView,
            BatchDocumentCreatorView batchDocumentCreatorView, BatchDocumentCreatorViewModel batchDocumentCreatorViewModel)
            : base(regionManager, AppScreens.AccountView)
        {
            _regionManager = regionManager;
            _userService = userService;
            _applicationState = applicationState;
            _accountSelectorView = accountSelectorView;
            _accountSelectorViewModel = accountSelectorViewModel;
            _accountDetailsView = accountDetailsView;
            _documentCreatorView = documentCreatorView;
            _batchDocumentCreatorView = batchDocumentCreatorView;
            _batchDocumentCreatorViewModel = batchDocumentCreatorViewModel;

            AddDashboardCommand<EntityCollectionViewModelBase<AccountTypeViewModel, AccountType>>(Resources.AccountType.ToPlural(), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountViewModel, Account>>(Resources.Account.ToPlural(), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountScreenViewModel, AccountScreen>>(Resources.AccountScreen.ToPlural(), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionTypeViewModel, AccountTransactionType>>(Resources.TransactionType.ToPlural(), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionDocumentTypeViewModel, AccountTransactionDocumentType>>(Resources.DocumentType.ToPlural(), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionDocumentViewModel, AccountTransactionDocument>>(Resources.Transaction.ToPlural(), Resources.Accounts, 40);

            PermissionRegistry.RegisterPermission(PermissionNames.NavigateAccountView, PermissionCategories.Navigation, Resources.CanNavigateCash);
            PermissionRegistry.RegisterPermission(PermissionNames.CreateAccount, PermissionCategories.Account, Resources.CanCreateAccount);

            SetNavigationCommand(Resources.Accounts, Resources.Common, "Images/Xls.png", 30);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountSelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountDetailsView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(DocumentCreatorView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(BatchDocumentCreatorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<AccountTransactionDocumentType>>().Subscribe(OnTransactionDocumentEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<DocumentCreationData>>().Subscribe(OnDocumentCreationData);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OperationRequest<AccountData>>>().Subscribe(OnAccountDataEvent);
        }


        private void OnTransactionDocumentEvent(EventParameters<AccountTransactionDocumentType> ep)
        {
            switch (ep.Topic)
            {
                case EventTopicNames.BatchCreateDocument:
                    _batchDocumentCreatorViewModel.Update(ep.Value);
                    ActivateBatchDocumentCreator();
                    break;
                case EventTopicNames.BatchDocumentsCreated:
                    ActivateAccountSelector();
                    break;
            }
        }

        private void OnDocumentCreationData(EventParameters<DocumentCreationData> ep)
        {
            if (ep.Topic == EventTopicNames.AccountTransactionDocumentSelected)
            {
                ActivateDocumentCreator();
            }
        }

        private void OnAccountDataEvent(EventParameters<OperationRequest<AccountData>> ep)
        {
            switch (ep.Topic)
            {
                case EventTopicNames.DisplayAccountTransactions: ActivateAccountTransactions(); break;
                case EventTopicNames.ActivateAccountSelector: ActivateAccountSelector(); break;
            }
        }

        private void ActivateBatchDocumentCreator()
        {
            _regionManager.ActivateRegion(RegionNames.MainRegion, _batchDocumentCreatorView);
        }

        private void ActivateDocumentCreator()
        {
            _regionManager.ActivateRegion(RegionNames.MainRegion, _documentCreatorView);
        }

        private void ActivateAccountTransactions()
        {
            _regionManager.ActivateRegion(RegionNames.MainRegion, _accountDetailsView);
        }

        private void ActivateAccountSelector()
        {
            _regionManager.ActivateRegion(RegionNames.MainRegion, _accountSelectorView);
            _accountSelectorViewModel.Refresh();
        }

        public override object GetVisibleView()
        {
            return _accountSelectorView;
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.NavigateAccountView) && _applicationState.CurrentWorkPeriod != null;
        }

        protected override void OnNavigate(string obj)
        {
            Activate();
            _accountSelectorViewModel.Refresh();
            base.OnNavigate(obj);
        }
    }
}
