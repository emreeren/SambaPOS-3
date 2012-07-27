using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class AccountSelectorViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountSelectorViewModel(IAccountService accountService, ICacheService cacheService)
        {
            _accountService = accountService;
            _cacheService = cacheService;
            ShowAccountDetailsCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(' ', '\r'), OnShowAccountDetails, CanShowAccountDetails);
            AccountButtonSelectedCommand = new CaptionCommand<AccountScreen>("", OnAccountScreenSelected);
            if (AccountScreens.Count() > 0)
                UpdateAccountScreen(AccountScreens.First());

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.ResetCache)
                {
                    _accountButtons = null;
                }
            });
        }

        private void OnAccountScreenSelected(AccountScreen accountScreen)
        {
            UpdateAccountScreen(accountScreen);
        }

        private void UpdateAccountScreen(AccountScreen accountScreen)
        {
            _accounts =
                _accountService.GetAccounts(
                    _cacheService.GetAccountTemplatesByName(accountScreen.AccountTemplateNamesList).ToArray());
            RaisePropertyChanged(() => Accounts);
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

        private IEnumerable<Account> _accounts;
        public IEnumerable<Account> Accounts
        {
            get { return _accounts; }
        }

        public ICaptionCommand ShowAccountDetailsCommand { get; set; }
        public ICaptionCommand AccountButtonSelectedCommand { get; set; }

        public Account SelectedAccount { get; set; }

        private bool CanShowAccountDetails(string arg)
        {
            return SelectedAccount != null;
        }

        private void OnShowAccountDetails(object obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData { AccountId = SelectedAccount.Id }, EventTopicNames.DisplayAccountTransactions, EventTopicNames.ActivateAccountSelector);
        }
    }
}
