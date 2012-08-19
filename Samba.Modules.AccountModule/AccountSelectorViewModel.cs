using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data.Specification;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    public class AccountRowData
    {
        public AccountRowData(Account account, decimal balance)
        {
            Account = account;
            Balance = balance;
        }

        public string BalanceStr { get { return Balance.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public Account Account { get; set; }
        public decimal Balance { get; set; }
        public string Fill { get; set; }
        public string AccountName { get { return Account != null ? Account.Name : ""; } }
    }

    [Export]
    public class AccountSelectorViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private AccountScreen _selectedAccountScreen;

        [ImportingConstructor]
        public AccountSelectorViewModel(IAccountService accountService, ICacheService cacheService, IApplicationState applicationState)
        {
            _accountService = accountService;
            _cacheService = cacheService;
            _applicationState = applicationState;
            ShowAccountDetailsCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(' ', '\r'), OnShowAccountDetails, CanShowAccountDetails);
            AccountButtonSelectedCommand = new CaptionCommand<AccountScreen>("", OnAccountScreenSelected);

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

        private IEnumerable<DocumentTemplateButtonViewModel> _batchDocumentButtons;
        public IEnumerable<DocumentTemplateButtonViewModel> BatchDocumentButtons
        {
            get
            {
                return _batchDocumentButtons ??
                    (_batchDocumentButtons =
                    _selectedAccountScreen != null
                    ? _cacheService.GetBatchDocumentTemplates(_selectedAccountScreen.AccountTemplateNamesList)
                         .Select(x => new DocumentTemplateButtonViewModel(x, null)) : null);
            }
        }

        private void OnAccountScreenSelected(AccountScreen accountScreen)
        {
            UpdateAccountScreen(accountScreen);
        }

        private void UpdateAccountScreen(AccountScreen accountScreen)
        {
            if (accountScreen == null) return;
            _batchDocumentButtons = null;
            _selectedAccountScreen = accountScreen;
            var accountBalances = _accountService.GetAccountsWithBalances(_cacheService.GetAccountTemplatesByName(accountScreen.AccountTemplateNamesList), GetFilter());
            _accounts = accountBalances.Select(x => new AccountRowData(x.Key, x.Value));
            RaisePropertyChanged(() => Accounts);
            RaisePropertyChanged(() => BatchDocumentButtons);
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

        private IEnumerable<AccountRowData> _accounts;
        public IEnumerable<AccountRowData> Accounts
        {
            get { return _accounts; }
        }

        public ICaptionCommand ShowAccountDetailsCommand { get; set; }
        public ICaptionCommand AccountButtonSelectedCommand { get; set; }

        public AccountRowData SelectedAccount { get; set; }

        private Expression<Func<AccountTransactionValue, bool>> GetFilter()
        {
            if (_selectedAccountScreen == null || _selectedAccountScreen.Filter == 0) return null;
            //Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod
            if (_selectedAccountScreen.Filter == 1) return x => x.Date >= DateTime.Now.MonthStart();
            if (_selectedAccountScreen.Filter == 3) return x => x.Date >= _applicationState.CurrentWorkPeriod.StartDate;
            return null;
        }

        private bool CanShowAccountDetails(string arg)
        {
            return SelectedAccount != null;
        }

        private void OnShowAccountDetails(object obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData { AccountId = SelectedAccount.Account.Id }, EventTopicNames.DisplayAccountTransactions, EventTopicNames.ActivateAccountSelector);
        }

        public void Refresh()
        {
            UpdateAccountScreen(_selectedAccountScreen ?? (_selectedAccountScreen = AccountScreens.FirstOrDefault()));
        }
    }
}
