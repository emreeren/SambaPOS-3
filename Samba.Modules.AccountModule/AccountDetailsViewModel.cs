using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class AccountDetailsViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountDetailsViewModel(IUserService userService, IApplicationState applicationState, ICacheService cacheService)
        {
            _userService = userService;
            _applicationState = applicationState;
            _cacheService = cacheService;
            MakePaymentToAccountCommand = new CaptionCommand<string>(Resources.MakePayment_r, OnMakePaymentToAccountCommand, CanMakePaymentToAccount);
            GetPaymentFromAccountCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnGetPaymentFromAccountCommand, CanMakePaymentToAccount);
            AddLiabilityCommand = new CaptionCommand<string>(Resources.AddLiability_r, OnAddLiability, CanAddLiability);
            AddReceivableCommand = new CaptionCommand<string>(Resources.AddReceivable_r, OnAddReceivable, CanAddLiability);
            CloseAccountScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseAccountScreen);
            AccountDetails = new ObservableCollection<AccountDetailViewModel>();
            DocumentTemplates = new ObservableCollection<DocumentTemplateButtonViewModel>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(OnDisplayAccountTransactions);
        }

        private AccountSearchViewModel _selectedAccount;
        public AccountSearchViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
                DisplayTransactions();
                UpdateTemplates();
            }
        }

        public ObservableCollection<DocumentTemplateButtonViewModel> DocumentTemplates { get; set; }
        public ObservableCollection<AccountDetailViewModel> AccountDetails { get; set; }

        public string TotalDebit { get { return AccountDetails.Sum(x => x.Debit).ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string TotalCredit { get { return AccountDetails.Sum(x => x.Credit).ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string TotalBalance { get { return AccountDetails.Sum(x => x.Debit - x.Credit).ToString(LocalSettings.DefaultCurrencyFormat); } }

        public ICaptionCommand GetPaymentFromAccountCommand { get; set; }
        public ICaptionCommand MakePaymentToAccountCommand { get; set; }
        public ICaptionCommand AddReceivableCommand { get; set; }
        public ICaptionCommand AddLiabilityCommand { get; set; }
        public ICaptionCommand CloseAccountScreenCommand { get; set; }

        private void UpdateTemplates()
        {
            DocumentTemplates.Clear();
            if (SelectedAccount != null)
            {
                var templates = _cacheService.GetAccountTransactionDocumentTemplates(SelectedAccount.Model.AccountTemplateId);
                DocumentTemplates.AddRange(templates.Select(x => new DocumentTemplateButtonViewModel(x, SelectedAccount.Model)));
            }
        }

        private void DisplayTransactions()
        {
            AccountDetails.Clear();
            var transactions = Dao.Query<AccountTransactionValue>(x => x.AccountId == SelectedAccount.Id);

            AccountDetails.AddRange(transactions.Select(x => new AccountDetailViewModel(x)));

            for (var i = 0; i < AccountDetails.Count; i++)
            {
                AccountDetails[i].Balance = (AccountDetails[i].Debit - AccountDetails[i].Credit);
                if (i > 0) (AccountDetails[i].Balance) += (AccountDetails[i - 1].Balance);
            }

            RaisePropertyChanged(() => AccountDetails);
            RaisePropertyChanged(() => TotalCredit);
            RaisePropertyChanged(() => TotalDebit);
            RaisePropertyChanged(() => TotalBalance);
        }

        private void OnDisplayAccountTransactions(EventParameters<Account> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayAccountTransactions)
            {
                SelectedAccount = new AccountSearchViewModel(obj.Value, null);
            }
        }

        private bool CanAddLiability(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.CreditOrDeptAccount);
        }

        private bool CanMakePaymentToAccount(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.MakeAccountTransaction);
        }

        private void OnCloseAccountScreen(string obj)
        {
            AccountDetails.Clear();
            EventServiceFactory.EventService.PublishEvent(_applicationState.CurrentTicket != null
                                                              ? EventTopicNames.ActivateTicket
                                                              : EventTopicNames.ActivateAccountView);
        }

        private void OnAddReceivable(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.AddReceivableAmount);
        }

        private void OnAddLiability(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.AddLiabilityAmount);
        }

        private void OnGetPaymentFromAccountCommand(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.GetPaymentFromAccount);
        }

        private void OnMakePaymentToAccountCommand(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.MakePaymentToAccount);
        }

    }
}
