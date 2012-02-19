using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    public class AccountDetailViewModel : ObservableObject
    {
        public AccountTransactionValue Model { get; set; }

        public AccountDetailViewModel(AccountTransactionValue model)
        {
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

        public decimal Debit { get { return Model.Debit; } }
        public decimal Credit { get { return Model.Credit; } }
        public decimal Balance { get; set; }

        public string DebitStr { get { return Debit.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string CreditStr { get { return Credit.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string BalanceStr { get { return Balance.ToString(LocalSettings.DefaultCurrencyFormat); } }
    }
}
