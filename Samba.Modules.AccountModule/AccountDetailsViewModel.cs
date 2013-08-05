using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class AccountDetailsViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;
        private readonly IReportService _reportService;

        private OperationRequest<AccountData> _currentOperationRequest;

        [ImportingConstructor]
        public AccountDetailsViewModel(IApplicationState applicationState, IAccountService accountService,
            ICacheService cacheService, IReportService reportService)
        {
            _applicationState = applicationState;
            _accountService = accountService;
            _cacheService = cacheService;
            _reportService = reportService;
            CloseAccountScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseAccountScreen);
            DisplayTicketCommand = new CaptionCommand<string>(Resources.FindTicket.Replace(" ", "\r"), OnDisplayTicket);
            PrintAccountCommand = new CaptionCommand<string>(Resources.Print, OnPrintAccount);
            AccountDetails = new ObservableCollection<AccountDetailData>();
            DocumentTypes = new ObservableCollection<DocumentTypeButtonViewModel>();
            AccountSummaries = new ObservableCollection<AccountSummaryData>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OperationRequest<AccountData>>>().Subscribe(OnDisplayAccountTransactions);
        }

        public AccountType SelectedAccountType { get; set; }
        public AccountDetailData FocusedAccountTransaction { get; set; }

        private Account _selectedAccount;
        public Account SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                _selectedAccount = value;
                if (SelectedAccountType == null || SelectedAccountType.Id != _selectedAccount.AccountTypeId)
                {
                    SelectedAccountType = _cacheService.GetAccountTypeById(value.AccountTypeId);
                }
                RaisePropertyChanged(() => SelectedAccount);
                FilterType = FilterTypes[SelectedAccountType.DefaultFilterType];
                UpdateTemplates();
            }
        }

        public ObservableCollection<DocumentTypeButtonViewModel> DocumentTypes { get; set; }
        public ObservableCollection<AccountDetailData> AccountDetails { get; set; }
        public ObservableCollection<AccountSummaryData> AccountSummaries { get; set; }

        public string[] FilterTypes { get { return new[] { Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod }; } }

        private string _filterType;
        public string FilterType
        {
            get { return _filterType; }
            set
            {
                _filterType = value;
                DisplayTransactions();
            }
        }

        public string TotalBalance { get { return AccountDetails.Sum(x => x.Debit - x.Credit).ToString(LocalSettings.ReportCurrencyFormat); } }

        public ICaptionCommand CloseAccountScreenCommand { get; set; }
        public ICaptionCommand DisplayTicketCommand { get; set; }
        public ICaptionCommand PrintAccountCommand { get; set; }

        private void UpdateTemplates()
        {
            DocumentTypes.Clear();
            if (SelectedAccount != null)
            {
                var templates = _applicationState.GetAccountTransactionDocumentTypes(SelectedAccount.AccountTypeId)
                    .Where(x => !string.IsNullOrEmpty(x.ButtonHeader) && x.CanMakeAccountTransaction(SelectedAccount));
                DocumentTypes.AddRange(templates.Select(x => new DocumentTypeButtonViewModel(x, SelectedAccount)));
            }
        }

        private void DisplayTransactions()
        {
            var transactionData = _accountService.GetAccountTransactionSummary(SelectedAccount, _applicationState.CurrentWorkPeriod);

            AccountDetails.Clear();
            AccountDetails.AddRange(transactionData.Transactions);
            AccountSummaries.Clear();
            AccountSummaries.AddRange(transactionData.Summaries);

            RaisePropertyChanged(() => TotalBalance);
        }

        private void OnDisplayAccountTransactions(EventParameters<OperationRequest<AccountData>> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayAccountTransactions)
            {
                var account = _accountService.GetAccountById(obj.Value.SelectedItem.AccountId);
                if (obj.Value != null && !string.IsNullOrEmpty(obj.Value.GetExpectedEvent()))
                    _currentOperationRequest = obj.Value;
                SelectedAccount = account;
            }
        }

        private void OnCloseAccountScreen(string obj)
        {
            AccountDetails.Clear();
            if (_currentOperationRequest != null)
                _currentOperationRequest.Publish(new AccountData(SelectedAccount));
        }

        private void OnDisplayTicket(string obj)
        {
            if (FocusedAccountTransaction != null)
            {
                var did = FocusedAccountTransaction.Model.AccountTransactionDocumentId;
                var ticket = Dao.Single<Ticket>(x => x.TransactionDocument.Id == did);
                if (ticket != null)
                {
                    string expectedEvent = _currentOperationRequest != null
                                               ? _currentOperationRequest.GetExpectedEvent()
                                               : EventTopicNames.DisplayAccountTransactions;

                    ExtensionMethods.PublishIdEvent(ticket.Id,
                        EventTopicNames.DisplayTicket,
                        () => CommonEventPublisher.PublishEntityOperation(new AccountData(SelectedAccount), EventTopicNames.DisplayAccountTransactions, expectedEvent));
                }
            }
        }

        private void OnPrintAccount(string obj)
        {
            _reportService.PrintAccountTransactions(SelectedAccount, _applicationState.CurrentWorkPeriod, _applicationState.GetReportPrinter());
        }
    }
}
