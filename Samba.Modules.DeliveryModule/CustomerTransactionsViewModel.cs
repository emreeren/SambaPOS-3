using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class CustomerTransactionsViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ImportingConstructor]
        public CustomerTransactionsViewModel(IUserService userService)
        {
            _userService = userService;

            MakePaymentToAccountCommand = new CaptionCommand<string>(Resources.MakePayment_r, OnMakePaymentToAccountCommand, CanMakePaymentToAccount);
            GetPaymentFromAccountCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnGetPaymentFromAccountCommand, CanMakePaymentToAccount);
            AddLiabilityCommand = new CaptionCommand<string>(Resources.AddLiability_r, OnAddLiability, CanAddLiability);
            AddReceivableCommand = new CaptionCommand<string>(Resources.AddReceivable_r, OnAddReceivable, CanAddLiability);
            CloseAccountScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseAccountScreen);
            SelectedCustomerTransactions = new ObservableCollection<CustomerTransactionViewModel>();

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(OnDisplayAccountTransactions);
        }

        private void OnDisplayAccountTransactions(EventParameters<Account> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayAccountTransactions)
            {
                SelectedAccount = new AccountSearchViewModel(obj.Value);
            }
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
            }
        }

        public ObservableCollection<CustomerTransactionViewModel> SelectedCustomerTransactions { get; set; }

        public string TotalReceivable { get { return SelectedCustomerTransactions.Sum(x => x.Receivable).ToString("#,#0.00"); } }
        public string TotalLiability { get { return SelectedCustomerTransactions.Sum(x => x.Liability).ToString("#,#0.00"); } }
        public string TotalBalance { get { return SelectedCustomerTransactions.Sum(x => x.Receivable - x.Liability).ToString("#,#0.00"); } }

        public ICaptionCommand GetPaymentFromAccountCommand { get; set; }
        public ICaptionCommand MakePaymentToAccountCommand { get; set; }
        public ICaptionCommand AddReceivableCommand { get; set; }
        public ICaptionCommand AddLiabilityCommand { get; set; }
        public ICaptionCommand CloseAccountScreenCommand { get; set; }

        private void DisplayTransactions()
        {
            SelectedCustomerTransactions.Clear();
            if (SelectedAccount != null)
            {
                var tickets = Dao.Query<Ticket>(x => x.AccountId == SelectedAccount.Id && x.LastPaymentDate > SelectedAccount.AccountOpeningDate, x => x.Payments);
                var cashTransactions = Dao.Query<CashTransaction>(x => x.Date > SelectedAccount.AccountOpeningDate && x.AccountId == SelectedAccount.Id);
                var customerTransactions = Dao.Query<CustomerTransaction>(x => x.Date > SelectedAccount.AccountOpeningDate && x.AccountId == SelectedAccount.Id);

                var transactions = new List<CustomerTransactionViewModel>();
                transactions.AddRange(tickets.Select(x => new CustomerTransactionViewModel
                {
                    Description = string.Format(Resources.TicketNumber_f, x.TicketNumber),
                    Date = x.LastPaymentDate,
                    Receivable = x.GetAccountPaymentAmount() + x.GetAccountRemainingAmount(),
                    Liability = x.GetAccountPaymentAmount()
                }));

                transactions.AddRange(cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Income)
                    .Select(x => new CustomerTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Liability = x.Amount
                    }));

                transactions.AddRange(cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Expense)
                    .Select(x => new CustomerTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Receivable = x.Amount
                    }));

                transactions.AddRange(customerTransactions.Where(x => x.TransactionType == (int)TransactionType.Liability)
                    .Select(x => new CustomerTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Liability = x.Amount
                    }));

                transactions.AddRange(customerTransactions.Where(x => x.TransactionType == (int)TransactionType.Receivable)
                    .Select(x => new CustomerTransactionViewModel
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

                SelectedCustomerTransactions.AddRange(transactions);
                RaisePropertyChanged(() => TotalReceivable);
                RaisePropertyChanged(() => TotalLiability);
                RaisePropertyChanged(() => TotalBalance);
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
            // RefreshSelectedAccount();
            SelectedCustomerTransactions.Clear();
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
