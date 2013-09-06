using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization;
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

        public ICaptionCommand RefreshCommand { get; set; }
        public ICaptionCommand CloseAccountScreenCommand { get; set; }
        public ICaptionCommand DisplayTicketCommand { get; set; }
        public ICaptionCommand PrintAccountCommand { get; set; }

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
            RefreshCommand = new CaptionCommand<string>(Resources.Refresh, OnRefreshCommand);
            AccountDetails = new ObservableCollection<AccountDetailData>();
            DocumentTypes = new ObservableCollection<DocumentTypeButtonViewModel>();
            AccountSummaries = new ObservableCollection<AccountSummaryData>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OperationRequest<AccountData>>>().Subscribe(OnDisplayAccountTransactions);
        }

        private DateTime? _start;
        public DateTime? Start
        {
            get { return _start; }
            set { _start = value; RaisePropertyChanged(() => StartDateString); }
        }

        private DateTime? _end;
        public DateTime? End
        {
            get { return _end; }
            set { _end = value; RaisePropertyChanged(() => EndDateString); }
        }

        public string StartDateString { get { return Start.HasValue ? Start.GetValueOrDefault().ToString("dd MM yyyy") : ""; } set { SetStartDate(value); } }
        public string EndDateString { get { return End.HasValue ? End.GetValueOrDefault().ToString("dd MM yyyy") : ""; } set { SetEndDate(value); } }

        private void SetStartDate(string value)
        {
            Start = StrToDate(value);
            if (!Start.HasValue) End = Start;
        }

        private void SetEndDate(string value)
        {
            End = StrToDate(value);
            if (Start.HasValue && End == Start.GetValueOrDefault())
            {
                End = Start.GetValueOrDefault().AddDays(1).AddSeconds(-1);
            }
            if (End.HasValue && End < Start) End = null;
        }

        private static DateTime? StrToDate(string value)
        {
            if (string.IsNullOrEmpty(value.Trim())) return null;
            var vals = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList();
            if (vals.Count == 1) vals.Add(DateTime.Now.Month);
            if (vals.Count == 2) vals.Add(DateTime.Now.Year);

            if (vals[2] < 1) { vals[2] = DateTime.Now.Year; }
            if (vals[2] < 1000) { vals[2] += 2000; }

            if (vals[1] < 1) { vals[1] = 1; }
            if (vals[1] > 12) { vals[1] = 12; }

            var dim = DateTime.DaysInMonth(vals[0], vals[1]);
            if (vals[0] < 1) { vals[0] = 1; }
            if (vals[0] > dim) { vals[0] = dim; }
            return new DateTime(vals[2], vals[1], vals[0]);
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
                UpdateTemplates();
                FilterType = Resources.Default;
            }
        }

        public ObservableCollection<DocumentTypeButtonViewModel> DocumentTypes { get; set; }
        public ObservableCollection<AccountDetailData> AccountDetails { get; set; }
        public ObservableCollection<AccountSummaryData> AccountSummaries { get; set; }

        public string[] FilterTypes { get { return new[] { Resources.Default, Resources.All, Resources.ThisMonth, Resources.PastMonth, Resources.ThisWeek, Resources.PastWeek, Resources.WorkPeriod }; } }

        private string _filterType;
        public string FilterType
        {
            get { return _filterType; }
            set
            {
                _filterType = value;
                Start = null;
                End = null;
                DisplayTransactions();
                RaisePropertyChanged(() => FilterType);
            }
        }

        public string TotalBalance { get { return AccountDetails.Sum(x => x.Debit - x.Credit).ToString(LocalSettings.ReportCurrencyFormat); } }

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
            if (FilterType != Resources.Default)
            {
                var dateRange = _accountService.GetDateRange(FilterType, _applicationState.CurrentWorkPeriod);
                Start = dateRange.Start;
                End = dateRange.End;
            }

            var transactionData = _accountService.GetAccountTransactionSummary(SelectedAccount, _applicationState.CurrentWorkPeriod, Start, End);
            Start = transactionData.Start;
            End = transactionData.End != transactionData.Start ? transactionData.End : null;

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

        private void OnRefreshCommand(string obj)
        {
            DisplayTransactions();
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
            _reportService.PrintAccountTransactions(SelectedAccount, _applicationState.CurrentWorkPeriod, _applicationState.GetReportPrinter(), FilterType);
        }
    }
}
