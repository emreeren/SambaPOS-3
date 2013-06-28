using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class AccountDetailsViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly IAccountService _accountService;
        private readonly IPrinterService _printerService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountDetailsViewModel(IApplicationState applicationState, IAccountService accountService,
            IPrinterService printerService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _accountService = accountService;
            _printerService = printerService;
            _cacheService = cacheService;
            CloseAccountScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseAccountScreen);
            DisplayTicketCommand = new CaptionCommand<string>(Resources.FindTicket.Replace(" ", "\r"), OnDisplayTicket);
            PrintAccountCommand = new CaptionCommand<string>(Resources.Print, OnPrintAccount);
            AccountDetails = new ObservableCollection<AccountDetailViewModel>();
            DocumentTypes = new ObservableCollection<DocumentTypeButtonViewModel>();
            AccountSummaries = new ObservableCollection<AccountSummaryViewModel>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<AccountData>>>().Subscribe(OnDisplayAccountTransactions);
        }

        public AccountType SelectedAccountType { get; set; }
        public AccountDetailViewModel FocusedAccountTransaction { get; set; }

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
        public ObservableCollection<AccountDetailViewModel> AccountDetails { get; set; }
        public ObservableCollection<AccountSummaryViewModel> AccountSummaries { get; set; }

        public string[] FilterTypes { get { return new[] { Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod }; } }

        private string _filterType;
        private EntityOperationRequest<AccountData> _currentOperationRequest;

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
                    .Where(x => !string.IsNullOrEmpty(x.ButtonHeader));
                DocumentTypes.AddRange(templates.Select(x => new DocumentTypeButtonViewModel(x, SelectedAccount)));
            }
        }

        private Expression<Func<AccountTransactionValue, bool>> GetCurrentRange(Expression<Func<AccountTransactionValue, bool>> activeSpecification)
        {
            if (FilterType == Resources.Month) return activeSpecification.And(x => x.Date >= DateTime.Now.MonthStart());
            if (FilterType == Resources.WorkPeriod) return activeSpecification.And(x => x.Date >= _applicationState.CurrentWorkPeriod.StartDate);
            return activeSpecification;
        }

        private Expression<Func<AccountTransactionValue, bool>> GetPastRange(Expression<Func<AccountTransactionValue, bool>> activeSpecification)
        {
            if (FilterType == Resources.Month) return activeSpecification.And(x => x.Date < DateTime.Now.MonthStart());
            if (FilterType == Resources.WorkPeriod) return activeSpecification.And(x => x.Date < _applicationState.CurrentWorkPeriod.StartDate);
            return activeSpecification;
        }

        private void DisplayTransactions()
        {
            AccountDetails.Clear();
            AccountSummaries.Clear();

            var transactions = Dao.Query(GetCurrentRange(x => x.AccountId == SelectedAccount.Id)).OrderBy(x => x.Date);
            AccountDetails.AddRange(transactions.Select(x => new AccountDetailViewModel(x, SelectedAccount)));

            if (FilterType != Resources.All)
            {
                var pastDebit = Dao.Sum(x => x.Debit, GetPastRange(x => x.AccountId == SelectedAccount.Id));
                var pastCredit = Dao.Sum(x => x.Credit, GetPastRange(x => x.AccountId == SelectedAccount.Id));
                var pastExchange = Dao.Sum(x => x.Exchange, GetPastRange(x => x.AccountId == SelectedAccount.Id));
                if (pastCredit > 0 || pastDebit > 0)
                {
                    AccountSummaries.Add(new AccountSummaryViewModel(Resources.Total, AccountDetails.Sum(x => x.Debit), AccountDetails.Sum(x => x.Credit)));
                    var detailValue =
                        new AccountDetailViewModel(new AccountTransactionValue
                                                       {
                                                           Name = Resources.PastTransactions,
                                                           Credit = pastCredit,
                                                           Debit = pastDebit,
                                                           Exchange = pastExchange
                                                       }, SelectedAccount);
                    AccountDetails.Insert(0, detailValue);
                    detailValue.IsBold = true;
                }
            }

            AccountSummaries.Add(new AccountSummaryViewModel(Resources.GrandTotal, AccountDetails.Sum(x => x.Debit), AccountDetails.Sum(x => x.Credit)));

            for (var i = 0; i < AccountDetails.Count; i++)
            {
                AccountDetails[i].Balance = (AccountDetails[i].Debit - AccountDetails[i].Credit);
                if (i > 0) (AccountDetails[i].Balance) += (AccountDetails[i - 1].Balance);
            }

            RaisePropertyChanged(() => TotalBalance);
        }

        private void OnDisplayAccountTransactions(EventParameters<EntityOperationRequest<AccountData>> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayAccountTransactions)
            {
                var account = _accountService.GetAccountById(obj.Value.SelectedEntity.AccountId);
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
            var report = new SimpleReport("");
            report.AddParagraph("Header");
            report.AddParagraphLine("Header", Resources.AccountTransaction.ToPlural(), true);
            report.AddParagraphLine("Header", "");
            report.AddParagraphLine("Header", string.Format("{0}: {1}", string.Format(Resources.Name_f, Resources.Account), SelectedAccount.Name));
            report.AddParagraphLine("Header", string.Format("{0}: {1}", Resources.Balance, TotalBalance));
            report.AddParagraphLine("Header", "");

            report.AddColumnLength("Transactions", "15*", "35*", "15*", "15*", "20*");
            report.AddColumTextAlignment("Transactions", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right);
            report.AddTable("Transactions", Resources.Date, Resources.Description, Resources.Credit, Resources.Debit, Resources.Balance);

            foreach (var ad in AccountDetails)
            {
                report.AddRow("Transactions", ad.Date.ToShortDateString(), ad.Name, ad.CreditStr, ad.DebitStr, ad.BalanceStr);
            }

            _printerService.PrintReport(report.Document, _applicationState.GetReportPrinter());
        }
    }
}
