﻿using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Modules.AccountModule.Dashboard;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;
        private readonly AccountSelectorView _accountSelectorView;
        private readonly AccountSelectorViewModel _accountSelectorViewModel;
        private readonly AccountDetailsView _accountDetailsView;
        private readonly DocumentCreatorView _documentCreatorView;
        private readonly BatchDocumentCreatorView _batchDocumentCreatorView;
        private readonly BatchDocumentCreatorViewModel _batchDocumentCreatorViewModel;

        [ImportingConstructor]
        public AccountModule(IRegionManager regionManager,
            IAutomationService automationService,
            IUserService userService,
            IAccountService accountService,
            ITicketService ticketService,
            ICacheService cacheService,
            AccountSelectorView accountSelectorView, AccountSelectorViewModel accountSelectorViewModel,
            AccountDetailsView accountDetailsView,
            DocumentCreatorView documentCreatorView,
            BatchDocumentCreatorView batchDocumentCreatorView, BatchDocumentCreatorViewModel batchDocumentCreatorViewModel)
            : base(regionManager, AppScreens.AccountView)
        {
            _regionManager = regionManager;
            _userService = userService;
            _accountService = accountService;
            _ticketService = ticketService;
            _cacheService = cacheService;
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

            automationService.RegisterActionType(ActionNames.CreateAccountTransactionDocument, string.Format(Resources.Create_f, Resources.AccountTransactionDocument), new { AccountTransactionDocumentName = "" });
            automationService.RegisterActionType(ActionNames.CreateAccountTransaction, string.Format(Resources.Create_f, Resources.AccountTransaction), new { AccountTransactionTypeName = "", Amount = 0m });

        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountSelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountDetailsView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(DocumentCreatorView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(BatchDocumentCreatorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<AccountTransactionDocumentType>>().Subscribe(OnTransactionDocumentEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<DocumentCreationData>>().Subscribe(OnDocumentCreationData);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<AccountData>>>().Subscribe(OnAccountDataEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<IActionData>>().Subscribe(OnActionData);
        }

        private void OnActionData(EventParameters<IActionData> ep)
        {
            if (ep.Value.Action.ActionType == ActionNames.CreateAccountTransactionDocument)
            {
                var documentName = ep.Value.GetAsString("AccountTransactionDocumentName");
                _accountService.CreateBatchAccountTransactionDocument(documentName);
            }
            if (ep.Value.Action.ActionType == ActionNames.CreateAccountTransaction)
            {
                var ticket = ep.Value.GetDataValue<Ticket>("Ticket");
                if (ticket != null)
                {
                    var amount = ep.Value.GetAsDecimal("Amount");
                    var transactionName = ep.Value.GetAsString("AccountTransactionTypeName");
                    if (!string.IsNullOrEmpty(transactionName))
                    {
                        var accountTransactionType = _cacheService.GetAccountTransactionTypeByName(transactionName);
                        if (accountTransactionType != null)
                        {
                            var ts = ticket.TicketEntities.FirstOrDefault(x => _ticketService.CanMakeAccountTransaction(x, accountTransactionType, 0));
                            if (ts != null)
                            {
                                //todo test
                                var account = _cacheService.GetAccountById(ts.AccountId);
                                ticket.TransactionDocument.AddNewTransaction(accountTransactionType, ticket.GetTicketAccounts(), amount, 1);
                            }
                        }
                    }
                }
            }
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

        private void OnAccountDataEvent(EventParameters<EntityOperationRequest<AccountData>> ep)
        {
            switch (ep.Topic)
            {
                case EventTopicNames.DisplayAccountTransactions: ActivateAccountTransactions(); break;
                case EventTopicNames.ActivateAccountSelector: ActivateAccountSelector(); break;
            }
        }

        private void ActivateBatchDocumentCreator()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_batchDocumentCreatorView);
        }

        private void ActivateDocumentCreator()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_documentCreatorView);
        }

        private void ActivateAccountTransactions()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountDetailsView);
        }

        private void ActivateAccountSelector()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountSelectorView);
        }

        public override object GetVisibleView()
        {
            return _accountSelectorView;
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.NavigateAccountView);
        }

        protected override void OnNavigate(string obj)
        {
            Activate();
            _accountSelectorViewModel.Refresh();
            base.OnNavigate(obj);
        }
    }
}
