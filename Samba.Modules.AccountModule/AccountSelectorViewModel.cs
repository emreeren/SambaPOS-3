using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class AccountSelectorViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IEntityService _entityService;
        private readonly IReportServiceClient _reportServiceClient;
        private AccountScreen _selectedAccountScreen;

        public event EventHandler Refreshed;

        protected virtual void OnRefreshed()
        {
            EventHandler handler = Refreshed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public ICaptionCommand ShowAccountDetailsCommand { get; set; }
        public ICaptionCommand PrintCommand { get; set; }
        public ICaptionCommand AccountButtonSelectedCommand { get; set; }
        public ICaptionCommand AutomationCommandSelectedCommand { get; set; }

        [ImportingConstructor]
        public AccountSelectorViewModel(IAccountService accountService, ICacheService cacheService, IApplicationState applicationState, IEntityService entityService,
            IReportServiceClient reportServiceClient)
        {
            _accounts = new ObservableCollection<AccountScreenRow>();
            _accountService = accountService;
            _cacheService = cacheService;
            _applicationState = applicationState;
            _entityService = entityService;
            _reportServiceClient = reportServiceClient;
            ShowAccountDetailsCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(' ', '\r'), OnShowAccountDetails, CanShowAccountDetails);
            PrintCommand = new CaptionCommand<string>(Resources.Print, OnPrint);
            AccountButtonSelectedCommand = new CaptionCommand<AccountScreen>("", OnAccountScreenSelected);
            AutomationCommandSelectedCommand = new CaptionCommand<AccountScreenAutmationCommandMap>("", OnAutomationCommandSelected);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.ResetCache)
                {
                    _accountButtons = null;
                    _batchDocumentButtons = null;
                    _selectedAccountScreen = null;
                }
            });
        }

        private IEnumerable<DocumentTypeButtonViewModel> _batchDocumentButtons;
        public IEnumerable<DocumentTypeButtonViewModel> BatchDocumentButtons
        {
            get
            {
                return _batchDocumentButtons ??
                    (_batchDocumentButtons =
                    _selectedAccountScreen != null
                    ? _applicationState.GetBatchDocumentTypes(_selectedAccountScreen.AccountScreenValues.Select(x => x.AccountTypeName))
                            .Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                            .Select(x => new DocumentTypeButtonViewModel(x, null)) : null);
            }
        }

        private IEnumerable<AccountScreenAutmationCommandMapViewModel> _automationCommands;
        public IEnumerable<AccountScreenAutmationCommandMapViewModel> AutomationCommands
        {
            get
            {
                return _automationCommands ?? (_automationCommands =
                    _selectedAccountScreen != null
                    ? _selectedAccountScreen.AutmationCommandMaps.Select(x => new AccountScreenAutmationCommandMapViewModel(x, _cacheService))
                    : null);
            }
        }

        public IEnumerable<AccountScreen> AccountScreens
        {
            get { return _cacheService.GetAccountScreens(); }
        }

        private IEnumerable<AccountButton> _accountButtons;
        public IEnumerable<AccountButton> AccountButtons
        {
            get { return _accountButtons ?? (_accountButtons = AccountScreens.Select(x => new AccountButton(x, _cacheService))); }
        }

        private readonly ObservableCollection<AccountScreenRow> _accounts;
        public ObservableCollection<AccountScreenRow> Accounts
        {
            get { return _accounts; }
        }

        public AccountScreenRow SelectedAccount { get; set; }


        private void OnAccountScreenSelected(AccountScreen accountScreen)
        {
            UpdateAccountScreen(accountScreen);
        }

        private bool CanShowAccountDetails(string arg)
        {
            return SelectedAccount != null && SelectedAccount.AccountId > 0;
        }

        private void OnShowAccountDetails(object obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData(SelectedAccount.AccountId), EventTopicNames.DisplayAccountTransactions, EventTopicNames.ActivateAccountSelector);
        }

        private void OnAutomationCommandSelected(AccountScreenAutmationCommandMap obj)
        {
            object value = null;
            if (obj.AutomationCommandValueType == 0) // Account Id
            {
                var account = _accountService.GetAccountById(SelectedAccount.AccountId);
                if (account == null) return;
                value = account.Id;
            }

            if (obj.AutomationCommandValueType == 1) //Entity Id
            {
                var entities = _entityService.GetEntitiesByAccountId(SelectedAccount.AccountId).ToList();
                if (!entities.Any()) return;
                value = entities.Select(x => x.Id).First();
            }

            if (obj.AutomationCommandValueType == 2) //Entity Id List
            {
                value = string.Join(",", _entityService.GetEntitiesByAccountId(SelectedAccount.AccountId).Select(x => x.Id));
            }

            _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new
            {
                obj.AutomationCommandName,
                CommandValue = value
            });
        }

        private void UpdateAccountScreen(AccountScreen accountScreen)
        {
            if (accountScreen == null) return;
            _batchDocumentButtons = null;
            _selectedAccountScreen = accountScreen;
            _automationCommands = null;

            _accounts.Clear();
            _accounts.AddRange(_accountService.GetAccountScreenRows(accountScreen, _applicationState.CurrentWorkPeriod));

            RaisePropertyChanged(() => BatchDocumentButtons);
            RaisePropertyChanged(() => AccountButtons);
            RaisePropertyChanged(() => AutomationCommands);

            OnRefreshed();
        }

        public void Refresh()
        {
            UpdateAccountScreen(_selectedAccountScreen ?? (_selectedAccountScreen = AccountScreens.FirstOrDefault()));
        }

        private void OnPrint(string obj)
        {
            _reportServiceClient.PrintAccountScreen(_selectedAccountScreen);
        }
    }
}
