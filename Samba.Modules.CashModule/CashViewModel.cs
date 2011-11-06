using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Transactions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.CashModule
{
    [Export]
    public class CashViewModel : ObservableObject
    {
        private IEnumerable<CashTransaction> _transactions;
        public IEnumerable<CashTransaction> Transactions
        {
            get { return _transactions ?? (_transactions = GetTransactions()); }
        }

        private IEnumerable<ICashTransactionViewModel> _incomeTransactions;
        public IEnumerable<ICashTransactionViewModel> IncomeTransactions
        {
            get
            {
                if (_incomeTransactions == null)
                {
                    _incomeTransactions = Transactions.Where(x => x.TransactionType == (int)TransactionType.Income).Select(
                            x => new CashTransactionViewModel(x)).OrderByDescending(x => x.Date).Cast<ICashTransactionViewModel>().ToList();

                    var operations = AppServices.CashService.GetCurrentCashOperationData();
                    var operationViewModel = new CashOperationViewModel
                                                 {
                                                     CashPaymentValue = operations[0],
                                                     CreditCardPaymentValue = operations[1],
                                                     TicketPaymentValue = operations[2],
                                                     Description = Resources.OperationIncome,
                                                     Date = DateTime.Now
                                                 };
                    (_incomeTransactions as IList).Insert(0, operationViewModel);

                    if (AppServices.MainDataContext.CurrentWorkPeriod != null)
                    {
                        var dayStartCashViewModel = new CashOperationViewModel
                                                        {
                                                            CashPaymentValue = AppServices.MainDataContext.CurrentWorkPeriod.CashAmount,
                                                            CreditCardPaymentValue = AppServices.MainDataContext.CurrentWorkPeriod.CreditCardAmount,
                                                            TicketPaymentValue = AppServices.MainDataContext.CurrentWorkPeriod.TicketAmount,
                                                            Description = Resources.StartOfDay,
                                                            Date = AppServices.MainDataContext.CurrentWorkPeriod.StartDate
                                                        };
                        (_incomeTransactions as IList).Insert(0, dayStartCashViewModel);
                    }
                }
                return _incomeTransactions;

            }
        }

        private IEnumerable<ICashTransactionViewModel> _expenseTransactions;
        public IEnumerable<ICashTransactionViewModel> ExpenseTransactions
        {
            get
            {
                return _expenseTransactions ??
                       (_expenseTransactions =
                        Transactions.Where(x => x.TransactionType == (int)TransactionType.Expense).Select(
                            x => new CashTransactionViewModel(x)).ToList().OrderByDescending(x => x.Date));
            }
        }

        private ICashTransactionViewModel _selectedIncomeTransaction;
        public ICashTransactionViewModel SelectedIncomeTransaction
        {
            get { return _selectedIncomeTransaction; }
            set
            {
                _selectedIncomeTransaction = value;
                foreach (var cashTransactionViewModel in _incomeTransactions)
                    cashTransactionViewModel.IsSelected = cashTransactionViewModel == value;
            }
        }

        private ICashTransactionViewModel _selectedExpenseTransaction;
        public ICashTransactionViewModel SelectedExpenseTransaction
        {
            get { return _selectedExpenseTransaction; }
            set
            {
                _selectedExpenseTransaction = value;
                foreach (var cashTransactionViewModel in _expenseTransactions)
                    cashTransactionViewModel.IsSelected = cashTransactionViewModel == value;
            }
        }

        private AccountViewModel _selectedAccount;
        public AccountViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                _selectedAccount = value;
                RaisePropertyChanged(() => IsAccountDetailsVisible);
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        public ICaptionCommand ActivateIncomeTransactionRecordCommand { get; set; }
        public ICaptionCommand ActivateExpenseTransactionRecordCommand { get; set; }
        public ICaptionCommand ApplyCashTransactionCommand { get; set; }
        public ICaptionCommand ApplyCreditCardTransactionCommand { get; set; }
        public ICaptionCommand ApplyTicketTransactionCommand { get; set; }
        public ICaptionCommand ApplyTransactionCommand { get; set; }
        public ICaptionCommand CancelTransactionCommand { get; set; }
        public ICaptionCommand DisplayAccountsCommand { get; set; }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged(() => Description); }
        }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; RaisePropertyChanged(() => Amount); }
        }

        private TransactionType _transactionType;
        public TransactionType TransactionType
        {
            get { return _transactionType; }
            set { _transactionType = value; RaisePropertyChanged(() => TransactionDescription); }
        }

        public string TransactionDescription
        {
            get
            {
                switch (TransactionType)
                {
                    case TransactionType.Income:
                        return Resources.NewIncomeTransaction;
                    case TransactionType.Expense:
                        return Resources.NewExpenseTransaction;
                    case TransactionType.Liability:
                        return Resources.NewLiabilityTransaction;
                    case TransactionType.Receivable:
                        return Resources.NewReceivableTransaction;
                }
                throw new Exception("Invalid Transaction Type");
            }
        }

        private int _activeView;
        public int ActiveView
        {
            get { return _activeView; }
            set
            {
                _activeView = value;
                RaisePropertyChanged(() => ActiveView);
                RaisePropertyChanged(() => IsPaymentButtonsVisible);
                RaisePropertyChanged(() => IsTransactionButtonVisible);
            }
        }

        public bool IsAccountDetailsVisible { get { return SelectedAccount != null; } }
        public bool IsPaymentButtonsVisible { get { return TransactionType == TransactionType.Income || TransactionType == TransactionType.Expense; } }
        public bool IsTransactionButtonVisible { get { return !IsPaymentButtonsVisible; } }
        public decimal CashIncomeTotal { get { return IncomeTransactions.Sum(x => x.CashPaymentValue); } }
        public decimal CreditCardIncomeTotal { get { return IncomeTransactions.Sum(x => x.CreditCardPaymentValue); } }
        public decimal TicketIncomeTotal { get { return IncomeTransactions.Sum(x => x.TicketPaymentValue); } }
        public decimal CashExpenseTotal { get { return ExpenseTransactions.Sum(x => x.CashPaymentValue); } }
        public decimal CreditCardExpenseTotal { get { return ExpenseTransactions.Sum(x => x.CreditCardPaymentValue); } }
        public decimal TicketExpenseTotal { get { return ExpenseTransactions.Sum(x => x.TicketPaymentValue); } }
        public decimal CashTotal { get { return CashIncomeTotal - CashExpenseTotal; } }
        public decimal CreditCardTotal { get { return CreditCardIncomeTotal - CreditCardExpenseTotal; } }
        public decimal TicketTotal { get { return TicketIncomeTotal - TicketExpenseTotal; } }

        public CashViewModel()
        {
            ActivateIncomeTransactionRecordCommand = new CaptionCommand<string>(Resources.IncomeTransaction_r, OnActivateIncomeTransactionRecord, CanActivateIncomeTransactionRecord);
            ActivateExpenseTransactionRecordCommand = new CaptionCommand<string>(Resources.ExpenseTransaction_r, OnActivateExpenseTransactionRecord, CanActivateIncomeTransactionRecord);
            CancelTransactionCommand = new CaptionCommand<string>(Resources.Cancel, OnCancelTransaction);

            ApplyTransactionCommand = new CaptionCommand<string>(Resources.Save, OnApplyTransaction, CanApplyTransaction);
            ApplyCashTransactionCommand = new CaptionCommand<string>(Resources.Cash, OnApplyCashTransaction, CanApplyTransaction);
            ApplyCreditCardTransactionCommand = new CaptionCommand<string>(Resources.CreditCard, OnApplyCreditCardTransaction, CanApplyTransaction);
            ApplyTicketTransactionCommand = new CaptionCommand<string>(Resources.Voucher, OnApplyTicketTransaction, CanApplyTransaction);
            DisplayAccountsCommand = new CaptionCommand<string>(Resources.Accounts, OnDisplayAccounts);
        }

        private static void OnDisplayAccounts(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateAccountView);
        }

        private static bool CanActivateIncomeTransactionRecord(string arg)
        {
            return AppServices.MainDataContext.IsCurrentWorkPeriodOpen && AppServices.IsUserPermittedFor(PermissionNames.MakeCashTransaction);
        }

        private int GetSelectedAccountId()
        {
            return SelectedAccount != null ? SelectedAccount.Id : 0;
        }

        private void OnApplyTransaction(string obj)
        {
            if (TransactionType == TransactionType.Liability)
                AppServices.CashService.AddLiability(GetSelectedAccountId(), Amount, Description);
            else if (TransactionType == TransactionType.Receivable)
                AppServices.CashService.AddReceivable(GetSelectedAccountId(), Amount, Description);
            ActivateTransactionList();
        }

        private void OnApplyTicketTransaction(string obj)
        {
            if (TransactionType == TransactionType.Expense)
                AppServices.CashService.AddExpense(GetSelectedAccountId(), Amount, Description, PaymentType.Ticket);
            else
                AppServices.CashService.AddIncome(GetSelectedAccountId(), Amount, Description, PaymentType.Ticket);
            ActivateTransactionList();
        }

        private void OnApplyCreditCardTransaction(string obj)
        {
            if (TransactionType == TransactionType.Expense)
                AppServices.CashService.AddExpense(GetSelectedAccountId(), Amount, Description, PaymentType.CreditCard);
            else
                AppServices.CashService.AddIncome(GetSelectedAccountId(), Amount, Description, PaymentType.CreditCard);
            ActivateTransactionList();
        }

        private void OnApplyCashTransaction(string obj)
        {
            if (TransactionType == TransactionType.Expense)
                AppServices.CashService.AddExpense(GetSelectedAccountId(), Amount, Description, PaymentType.Cash);
            else
                AppServices.CashService.AddIncome(GetSelectedAccountId(), Amount, Description, PaymentType.Cash);
            ActivateTransactionList();
        }

        private bool CanApplyTransaction(string arg)
        {
            return AppServices.MainDataContext.IsCurrentWorkPeriodOpen && !string.IsNullOrEmpty(Description) && Amount != 0;
        }

        private void OnCancelTransaction(string obj)
        {
            ActivateTransactionList();
        }

        private void OnActivateIncomeTransactionRecord(object obj)
        {
            ResetTransactionData(TransactionType.Income);
        }

        private void OnActivateExpenseTransactionRecord(object obj)
        {
            ResetTransactionData(TransactionType.Expense);
        }

        internal void ResetTransactionData(TransactionType transactionType)
        {
            TransactionType = transactionType;
            Description = "";
            Amount = 0;
            ActiveView = 1;
        }

        public void ActivateTransactionList()
        {
            ResetTransactionData(TransactionType);
            ActiveView = 0;
            _transactions = null;
            _incomeTransactions = null;
            _expenseTransactions = null;

            if (SelectedAccount != null)
            {
                SelectedAccount.Model.PublishEvent(EventTopicNames.ActivateAccount);
                SelectedAccount = null;
                return;
            }

            RaisePropertyChanged(() => IncomeTransactions);
            RaisePropertyChanged(() => ExpenseTransactions);
            RaisePropertyChanged(() => CashIncomeTotal);
            RaisePropertyChanged(() => CreditCardIncomeTotal);
            RaisePropertyChanged(() => TicketIncomeTotal);
            RaisePropertyChanged(() => CashExpenseTotal);
            RaisePropertyChanged(() => CreditCardExpenseTotal);
            RaisePropertyChanged(() => TicketExpenseTotal);
            RaisePropertyChanged(() => CashTotal);
            RaisePropertyChanged(() => CreditCardTotal);
            RaisePropertyChanged(() => TicketTotal);
        }

        private static IEnumerable<CashTransaction> GetTransactions()
        {
            return AppServices.MainDataContext.CurrentWorkPeriod != null
                    ? AppServices.CashService.GetTransactions(AppServices.MainDataContext.CurrentWorkPeriod).ToList()
                    : new List<CashTransaction>();
        }

        public void MakePaymentToAccount(Account account)
        {
            ResetTransactionData(TransactionType.Expense);
            SelectedAccount = new AccountViewModel(account);
        }

        internal void GetPaymentFromAccount(Account account)
        {
            ResetTransactionData(TransactionType.Income);
            SelectedAccount = new AccountViewModel(account);
        }

        internal void AddLiabilityAmount(Account account)
        {
            ResetTransactionData(TransactionType.Liability);
            SelectedAccount = new AccountViewModel(account);
        }

        internal void AddReceivableAmount(Account account)
        {
            ResetTransactionData(TransactionType.Receivable);
            SelectedAccount = new AccountViewModel(account);
        }
    }
}
