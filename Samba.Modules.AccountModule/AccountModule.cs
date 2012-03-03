using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Modules.AccountModule.Dashboard;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : VisibleModuleBase
    {
        private readonly IUserService _userService;
        private readonly IRegionManager _regionManager;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly AccountSearchView _accountSearchView;
        private readonly AccountEditorView _accountEditorView;
        private readonly AccountDetailsView _accountDetailsView;
        private readonly DocumentCreatorView _documentCreatorView;

        [ImportingConstructor]
        public AccountModule(IRegionManager regionManager, IUserService userService,
            IApplicationStateSetter applicationStateSetter,
            AccountSearchView accountSearchView,
            AccountEditorView accountEditorView,
            AccountDetailsView accountDetailsView,
            DocumentCreatorView documentCreatorView)
            : base(regionManager, AppScreens.AccountView)
        {
            _userService = userService;
            _regionManager = regionManager;
            _accountSearchView = accountSearchView;
            _accountDetailsView = accountDetailsView;
            _accountEditorView = accountEditorView;
            _documentCreatorView = documentCreatorView;
            _applicationStateSetter = applicationStateSetter;

            AddDashboardCommand<EntityCollectionViewModelBase<AccountTemplateViewModel, AccountTemplate>>(Resources.AccountTemplateList, Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountViewModel, Account>>(Resources.AccountList, Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionTemplateViewModel, AccountTransactionTemplate>>(string.Format(Resources.List_f, "Transaction Template"), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionDocumentViewModel, AccountTransactionDocument>>(string.Format(Resources.List_f, "Transaction Document"), Resources.Accounts, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<AccountTransactionDocumentTemplateViewModel, AccountTransactionDocumentTemplate>>(string.Format(Resources.List_f, "Document Template"), Resources.Accounts, 40);

            PermissionRegistry.RegisterPermission(PermissionNames.MakeAccountTransaction, PermissionCategories.Cash, Resources.CanMakeAccountTransaction);
            PermissionRegistry.RegisterPermission(PermissionNames.CreditOrDeptAccount, PermissionCategories.Cash, Resources.CanMakeCreditOrDeptTransaction);

            SetNavigationCommand(Resources.Accounts, Resources.Common, "Images/Xls.png", 70);
            PermissionRegistry.RegisterPermission(PermissionNames.NavigateAccountView, PermissionCategories.Navigation, Resources.CanNavigateCash);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountDetailsView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountSearchView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(DocumentCreatorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<DocumentCreationData>>().Subscribe(x =>
                {
                    if (x.Topic == EventTopicNames.AccountTransactionDocumentSelected)
                    {
                        ActivateDocumentCreator();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectAccount)
                    ActivateAccountSelector();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Account>>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectAccount)
                    {
                        ActivateAccountSelector(x.Value);
                    }
                    if (x.Topic == EventTopicNames.EditAccountDetails)
                    {
                        ActivateAccountEditor();
                    }
                    if (x.Topic == EventTopicNames.DisplayAccountTransactions)
                    {
                        ActivateCustomerTransactions();
                    }
                }
                );

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccountView)
                    ActivateAccountSelector();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == EventTopicNames.SelectAccount)
                    {
                        Activate();
                        //((DeliveryViewModel)_accountSearchView.DataContext).SearchAccount(x.Value.DataObject as string);
                    }
                });
        }

        private void ActivateDocumentCreator()
        {
            Activate();
            _regionManager.Regions[RegionNames.MainRegion].Activate(_documentCreatorView);
        }

        private void ActivateAccountEditor()
        {
            Activate();
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountEditorView);
        }

        private void ActivateCustomerTransactions()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountDetailsView);
        }

        private void ActivateAccountSelector(EntityOperationRequest<Account> value = null)
        {
            Activate();
            ((AccountSearchViewModel)_accountSearchView.DataContext).RefreshSelectedAccount(value);
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountSearchView);
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.NavigateAccountView);
        }

        protected override void OnNavigate(string obj)
        {
            _applicationStateSetter.SetCurrentDepartment(0);
            ActivateAccountSelector();
            base.OnNavigate(obj);
        }

        public override object GetVisibleView()
        {
            return _accountSearchView;
        }
    }
}
