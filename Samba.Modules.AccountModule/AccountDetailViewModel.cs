using System;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    public class AccountDetailViewModel : ObservableObject
    {
        private readonly Account _account;
        public AccountTransactionValue Model { get; set; }

        public AccountDetailViewModel(AccountTransactionValue model, Account account)
        {
            _account = account;
            Model = model;
        }

        public string Name { get { return Model.Name; } }
        public DateTime Date { get { return Model.Date; } }

        private bool _isBold;
        public bool IsBold
        {
            get { return _isBold; }
            set { _isBold = value; RaisePropertyChanged(() => IsBold); }
        }

        public decimal Debit { get { return _account.ForeignCurrencyId > 0 && Model.Exchange > 0 ? Model.Exchange : Model.Debit; } }
        public decimal Credit { get { return _account.ForeignCurrencyId > 0 && Model.Exchange < 0 ? Model.Exchange : Model.Credit; } }
        public decimal Balance { get; set; }

        public string DebitStr { get { return Debit.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string CreditStr { get { return Credit.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string BalanceStr { get { return Balance.ToString(LocalSettings.DefaultCurrencyFormat); } }
    }
}
