using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class AccountSelectorViewModel : ObservableObject
    {
        private readonly Timer _updateTimer;

        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SelectAccountCommand { get; set; }
        public ICaptionCommand CreateAccountCommand { get; set; }
        public ICaptionCommand ResetAccountCommand { get; set; }
        public ICaptionCommand FindTicketCommand { get; set; }
        public ICaptionCommand MakePaymentCommand { get; set; }
        public ICaptionCommand DisplayAccountCommand { get; set; }
        public ICaptionCommand GetPaymentFromAccountCommand { get; set; }
        public ICaptionCommand MakePaymentToAccountCommand { get; set; }
        public ICaptionCommand AddReceivableCommand { get; set; }
        public ICaptionCommand AddLiabilityCommand { get; set; }
        public ICaptionCommand CloseAccountScreenCommand { get; set; }

        public ObservableCollection<AccountSearchViewModel> FoundAccounts { get; set; }
        public ObservableCollection<AccountTransactionViewModel> SelectedAccountTransactions { get; set; }

        private int _selectedView;
        public int SelectedView
        {
            get { return _selectedView; }
            set { _selectedView = value; RaisePropertyChanged(() => SelectedView); }
        }

        public AccountSearchViewModel SelectedAccount { get { return FoundAccounts.Count == 1 ? FoundAccounts[0] : FocusedAccount; } }

        private AccountSearchViewModel _focusedAccount;
        public AccountSearchViewModel FocusedAccount
        {
            get { return _focusedAccount; }
            set
            {
                _focusedAccount = value;
                RaisePropertyChanged(() => FocusedAccount);
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        private string _ticketSearchText;
        public string TicketSearchText
        {
            get { return _ticketSearchText; }
            set { _ticketSearchText = value; RaisePropertyChanged(() => TicketSearchText); }
        }

        private string _phoneNumberSearchText;
        public string PhoneNumberSearchText
        {
            get { return string.IsNullOrEmpty(_phoneNumberSearchText) ? null : _phoneNumberSearchText.TrimStart('+', '0'); }
            set
            {
                if (value != _phoneNumberSearchText)
                {
                    _phoneNumberSearchText = value;
                    RaisePropertyChanged(() => PhoneNumberSearchText);
                    ResetTimer();
                }
            }
        }

        private string _accountNameSearchText;
        public string AccountNameSearchText
        {
            get { return _accountNameSearchText; }
            set
            {
                if (value != _accountNameSearchText)
                {
                    _accountNameSearchText = value;
                    RaisePropertyChanged(() => AccountNameSearchText);
                    ResetTimer();
                }
            }
        }

        private string _addressSearchText;
        public string AddressSearchText
        {
            get { return _addressSearchText; }
            set
            {
                if (value != _addressSearchText)
                {
                    _addressSearchText = value;
                    RaisePropertyChanged(() => AddressSearchText);
                    ResetTimer();
                }
            }
        }

        public bool IsResetAccountVisible
        {
            get
            {
                return (_applicationState.CurrentTicket != null &&
                        _applicationState.CurrentTicket.AccountId > 0);
            }
        }

        public bool IsClearVisible
        {
            get
            {
                return (_applicationState.CurrentTicket != null &&
                        _applicationState.CurrentTicket.AccountId == 0);
            }
        }

        public bool IsMakePaymentVisible
        {
            get
            {
                return (_applicationState.CurrentTicket != null && _userService.IsUserPermittedFor(PermissionNames.MakePayment));
            }
        }

        private int _activeView;
        private readonly IApplicationState _applicationState;
        private readonly IUserService _userService;
        private readonly ITicketService _ticketService;

        public int ActiveView
        {
            get { return _activeView; }
            set { _activeView = value; RaisePropertyChanged(() => ActiveView); }
        }

        public string TotalReceivable { get { return SelectedAccountTransactions.Sum(x => x.Receivable).ToString("#,#0.00"); } }
        public string TotalLiability { get { return SelectedAccountTransactions.Sum(x => x.Liability).ToString("#,#0.00"); } }
        public string TotalBalance { get { return SelectedAccountTransactions.Sum(x => x.Receivable - x.Liability).ToString("#,#0.00"); } }

        [ImportingConstructor]
        public AccountSelectorViewModel(IApplicationState applicationState, IUserService userService,
            ITicketService ticketService)
        {
            _applicationState = applicationState;
            _userService = userService;
            _ticketService = ticketService;

            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;

            FoundAccounts = new ObservableCollection<AccountSearchViewModel>();
            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
            SelectAccountCommand = new CaptionCommand<string>(Resources.SelectAccount_r, OnSelectAccount, CanSelectAccount);
            CreateAccountCommand = new CaptionCommand<string>(Resources.NewAccount_r, OnCreateAccount, CanCreateAccount);
            FindTicketCommand = new CaptionCommand<string>(Resources.FindTicket_r, OnFindTicket, CanFindTicket);
            ResetAccountCommand = new CaptionCommand<string>(Resources.ResetAccount_r, OnResetAccount, CanResetAccount);
            MakePaymentCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnMakePayment, CanMakePayment);
            DisplayAccountCommand = new CaptionCommand<string>(Resources.Account, OnDisplayAccount, CanSelectAccount);
            MakePaymentToAccountCommand = new CaptionCommand<string>(Resources.MakePayment_r, OnMakePaymentToAccountCommand, CanMakePaymentToAccount);
            GetPaymentFromAccountCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnGetPaymentFromAccountCommand, CanMakePaymentToAccount);
            AddLiabilityCommand = new CaptionCommand<string>(Resources.AddLiability_r, OnAddLiability, CanAddLiability);
            AddReceivableCommand = new CaptionCommand<string>(Resources.AddReceivable_r, OnAddReceivable, CanAddLiability);
            CloseAccountScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseAccountScreen);

            SelectedAccountTransactions = new ObservableCollection<AccountTransactionViewModel>();
        }

        private bool CanAddLiability(string arg)
        {
            return CanSelectAccount(arg) && _userService.IsUserPermittedFor(PermissionNames.CreditOrDeptAccount);
        }

        private bool CanMakePaymentToAccount(string arg)
        {
            return CanSelectAccount(arg) && _userService.IsUserPermittedFor(PermissionNames.MakeAccountTransaction);
        }

        private void OnAddReceivable(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.AddReceivableAmount);
            FoundAccounts.Clear();
        }

        private void OnAddLiability(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.AddLiabilityAmount);
            FoundAccounts.Clear();
        }

        private void OnCloseAccountScreen(string obj)
        {
            RefreshSelectedAccount();
        }

        private void OnGetPaymentFromAccountCommand(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.GetPaymentFromAccount);
            FoundAccounts.Clear();
        }

        private void OnMakePaymentToAccountCommand(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.MakePaymentToAccount);
            FoundAccounts.Clear();
        }

        internal void DisplayAccount(Account account)
        {
            FoundAccounts.Clear();
            if (account != null)
                FoundAccounts.Add(new AccountSearchViewModel(account));
            RaisePropertyChanged(() => SelectedAccount);
            OnDisplayAccount("");
        }

        private void OnDisplayAccount(string obj)
        {
            SaveSelectedAccount();
            SelectedAccountTransactions.Clear();
            if (SelectedAccount != null)
            {
                var tickets = Dao.Query<Ticket>(x => x.AccountId == SelectedAccount.Id && x.LastPaymentDate > SelectedAccount.AccountOpeningDate, x => x.Payments);
                var cashTransactions = Dao.Query<CashTransaction>(x => x.Date > SelectedAccount.AccountOpeningDate && x.AccountId == SelectedAccount.Id);
                var accountTransactions = Dao.Query<AccountTransaction>(x => x.Date > SelectedAccount.AccountOpeningDate && x.AccountId == SelectedAccount.Id);

                var transactions = new List<AccountTransactionViewModel>();
                transactions.AddRange(tickets.Select(x => new AccountTransactionViewModel
                                                       {
                                                           Description = string.Format(Resources.TicketNumber_f, x.TicketNumber),
                                                           Date = x.LastPaymentDate,
                                                           Receivable = x.GetAccountPaymentAmount() + x.GetAccountRemainingAmount(),
                                                           Liability = x.GetAccountPaymentAmount()
                                                       }));

                transactions.AddRange(cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Income)
                    .Select(x => new AccountTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Liability = x.Amount
                    }));

                transactions.AddRange(cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Expense)
                    .Select(x => new AccountTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Receivable = x.Amount
                    }));

                transactions.AddRange(accountTransactions.Where(x => x.TransactionType == (int)TransactionType.Liability)
                    .Select(x => new AccountTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Liability = x.Amount
                    }));

                transactions.AddRange(accountTransactions.Where(x => x.TransactionType == (int)TransactionType.Receivable)
                    .Select(x => new AccountTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Receivable = x.Amount
                    }));

                transactions = transactions.OrderBy(x => x.Date).ToList();

                for (var i = 0; i < transactions.Count; i++)
                {
                    transactions[i].Balance = (transactions[i].Receivable - transactions[i].Liability);
                    if (i > 0) (transactions[i].Balance) += (transactions[i - 1].Balance);
                }

                SelectedAccountTransactions.AddRange(transactions);
                RaisePropertyChanged(() => TotalReceivable);
                RaisePropertyChanged(() => TotalLiability);
                RaisePropertyChanged(() => TotalBalance);
            }
            ActiveView = 1;
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedAccount != null && _applicationState.CurrentTicket != null;
        }

        private void OnMakePayment(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.PaymentRequestedForTicket);
            ClearSearchValues();
        }

        private bool CanResetAccount(string arg)
        {
            return _applicationState.CurrentTicket != null &&
                _applicationState.CurrentTicket.CanSubmit &&
                _applicationState.CurrentTicket.AccountId > 0;
        }

        private static void OnResetAccount(string obj)
        {
            Account.Null.PublishEvent(EventTopicNames.AccountSelectedForTicket);
        }

        private void OnFindTicket(string obj)
        {
            _ticketService.OpenTicketByTicketNumber(TicketSearchText);
            if (_applicationState.CurrentTicket != null)
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.DisplayTicketView);
            TicketSearchText = "";
        }

        private bool CanFindTicket(string arg)
        {
            return !string.IsNullOrEmpty(TicketSearchText) && _applicationState.CurrentTicket == null;
        }

        private bool CanCreateAccount(string arg)
        {
            return SelectedAccount == null;
        }

        private void OnCreateAccount(string obj)
        {
            FoundAccounts.Clear();
            var c = new Account
                        {
                            Name = AccountNameSearchText,
                            SearchString = PhoneNumberSearchText
                        };
            FoundAccounts.Add(new AccountSearchViewModel(c));
            SelectedView = 1;
            RaisePropertyChanged(() => SelectedAccount);
        }

        private bool CanSelectAccount(string arg)
        {
            return
                _applicationState.IsCurrentWorkPeriodOpen
                && SelectedAccount != null
                && !string.IsNullOrEmpty(SelectedAccount.PhoneNumber)
                && !string.IsNullOrEmpty(SelectedAccount.Name)
                && (_applicationState.CurrentTicket == null || _applicationState.CurrentTicket.AccountId == 0);
        }

        private void SaveSelectedAccount()
        {
            if (!SelectedAccount.IsNotNew)
            {
                var ws = WorkspaceFactory.Create();
                ws.Add(SelectedAccount.Model);
                ws.CommitChanges();
            }
        }

        private void OnSelectAccount(string obj)
        {
            SaveSelectedAccount();
            SelectedAccount.Model.PublishEvent(EventTopicNames.AccountSelectedForTicket);
            ClearSearchValues();
        }

        void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();
            UpdateFoundAccounts();
        }

        private void ResetTimer()
        {
            _updateTimer.Stop();

            if (!string.IsNullOrEmpty(PhoneNumberSearchText)
                || !string.IsNullOrEmpty(AccountNameSearchText)
                || !string.IsNullOrEmpty(AddressSearchText))
            {
                _updateTimer.Start();
            }
            else FoundAccounts.Clear();
        }

        private void UpdateFoundAccounts()
        {

            IEnumerable<Account> result = new List<Account>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                                     {
                                         bool searchPn = string.IsNullOrEmpty(PhoneNumberSearchText);
                                         bool searchCn = string.IsNullOrEmpty(AccountNameSearchText);

                                         result = Dao.Query<Account>(
                                             x =>
                                                (searchPn || x.SearchString.Contains(PhoneNumberSearchText)) &&
                                                (searchCn || x.Name.ToLower().Contains(AccountNameSearchText.ToLower())));
                                     };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundAccounts.Clear();
                                   FoundAccounts.AddRange(result.Select(x => new AccountSearchViewModel(x)));

                                   if (SelectedAccount != null && PhoneNumberSearchText == SelectedAccount.PhoneNumber)
                                   {
                                       SelectedView = 1;
                                       SelectedAccount.UpdateDetailedInfo();
                                   }

                                   RaisePropertyChanged(() => SelectedAccount);

                                   CommandManager.InvalidateRequerySuggested();
                               }));

                    };

                worker.RunWorkerAsync();
            }
        }

        private void OnCloseScreen(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(_applicationState.IsCurrentWorkPeriodOpen
                                                              ? EventTopicNames.DisplayTicketView
                                                              : EventTopicNames.ActivateNavigation);
            SelectedView = 0;
            ActiveView = 0;
            SelectedAccountTransactions.Clear();
        }

        public void RefreshSelectedAccount()
        {
            ClearSearchValues();

            if (_applicationState.CurrentTicket != null && _applicationState.CurrentTicket.AccountId > 0)
            {
                var account = Dao.SingleWithCache<Account>(x => x.Id == _applicationState.CurrentTicket.AccountId);
                if (account != null) FoundAccounts.Add(new AccountSearchViewModel(account));
                if (SelectedAccount != null)
                {
                    SelectedView = 1;
                    SelectedAccount.UpdateDetailedInfo();
                }
            }
            RaisePropertyChanged(() => SelectedAccount);
            RaisePropertyChanged(() => IsClearVisible);
            RaisePropertyChanged(() => IsResetAccountVisible);
            RaisePropertyChanged(() => IsMakePaymentVisible);
            ActiveView = 0;
            SelectedAccountTransactions.Clear();
        }

        private void ClearSearchValues()
        {
            FoundAccounts.Clear();
            SelectedView = 0;
            ActiveView = 0;
            PhoneNumberSearchText = "";
            AddressSearchText = "";
            AccountNameSearchText = "";
        }

        public void SearchAccount(string phoneNumber)
        {
            ClearSearchValues();
            PhoneNumberSearchText = phoneNumber;
            UpdateFoundAccounts();
        }
    }
}
