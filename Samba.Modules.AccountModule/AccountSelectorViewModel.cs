using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    public class AccountRowData
    {
        public AccountRowData(string name, decimal balance, decimal exchange, int accountId, string currencyFormat)
        {
            Name = name;
            Balance = balance;
            if (!string.IsNullOrEmpty(currencyFormat)) Exchange = exchange;
            CurrencyFormat = currencyFormat;
            AccountId = accountId;
        }

        protected string CurrencyFormat { get; set; }
        public int AccountId { get; set; }
        public string BalanceStr
        {
            get
            {
               // if (!string.IsNullOrEmpty(ExchangeStr)) return ExchangeStr;
                return Balance.ToString(LocalSettings.DefaultCurrencyFormat);
            }
        }
        public string ExchangeStr { get { return Exchange != Balance ? string.Format(CurrencyFormat, Exchange) : ""; } }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public decimal Exchange { get; set; }
        public string Fill { get; set; }
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
            _accounts = new ObservableCollection<AccountRowData>();
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
                    ? _cacheService.GetBatchDocumentTemplates(_selectedAccountScreen.AccountScreenValues.Select(x => x.AccountTemplateName))
                         .Select(x => new DocumentTemplateButtonViewModel(x, null)) : null);
            }
        }

        private void OnAccountScreenSelected(AccountScreen accountScreen)
        {
            UpdateAccountScreen(accountScreen);
        }

        private string GetCurrencyFormat(int currencyId)
        {
            return currencyId == 0 ? "" : _cacheService.GetForeignCurrencies().Single(x => x.Id == currencyId).CurrencySymbol;
        }

        private void UpdateAccountScreen(AccountScreen accountScreen)
        {
            if (accountScreen == null) return;
            _batchDocumentButtons = null;
            _selectedAccountScreen = accountScreen;
            _accounts.Clear();

            var detailedTemplateNames = accountScreen.AccountScreenValues.Where(x => x.DisplayDetails).Select(x => x.AccountTemplateId);
            _accountService.GetAccountBalances(detailedTemplateNames.ToList(), GetFilter()).ToList().ForEach(x => _accounts.Add(new AccountRowData(x.Key.Name, x.Value.Balance, x.Value.Exchange, x.Key.Id, GetCurrencyFormat(x.Key.ForeignCurrencyId))));

            var templateTotals = accountScreen.AccountScreenValues.Where(x => !x.DisplayDetails).Select(x => x.AccountTemplateId);
            _accountService.GetAccountTemplateBalances(templateTotals.ToList(), GetFilter()).ToList().ForEach(x => _accounts.Add(new AccountRowData(x.Key.Name, x.Value.Balance, x.Value.Exchange, 0, "")));

            RaisePropertyChanged(() => BatchDocumentButtons);
            RaisePropertyChanged(() => AccountButtons);
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

        private readonly ObservableCollection<AccountRowData> _accounts;
        public ObservableCollection<AccountRowData> Accounts
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
            return SelectedAccount != null && SelectedAccount.AccountId > 0;
        }

        private void OnShowAccountDetails(object obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData { AccountId = SelectedAccount.AccountId }, EventTopicNames.DisplayAccountTransactions, EventTopicNames.ActivateAccountSelector);
        }

        public void Refresh()
        {
            UpdateAccountScreen(_selectedAccountScreen ?? (_selectedAccountScreen = AccountScreens.FirstOrDefault()));
        }
    }
}
