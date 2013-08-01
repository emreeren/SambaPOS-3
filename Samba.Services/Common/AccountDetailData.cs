using System;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;

namespace Samba.Services.Common
{
    public class AccountDetailData 
    {
        private readonly Account _account;
        public AccountTransactionValue Model { get; set; }

        public AccountDetailData(AccountTransactionValue model, Account account)
        {
            _account = account;
            Model = model;
        }

        public string Name { get { return Model.Name; } }
        public DateTime Date { get { return Model.Date; } }

        public bool IsBold { get; set; }

        public decimal Debit { get { return _account.ForeignCurrencyId > 0 && Model.Exchange > 0 ? Math.Abs(Model.Exchange) : Model.Debit; } }
        public decimal Credit { get { return _account.ForeignCurrencyId > 0 && Model.Exchange < 0 ? Math.Abs(Model.Exchange) : Model.Credit; } }
        public decimal Balance { get; set; }

        public string DebitStr { get { return Debit.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string CreditStr { get { return Credit.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string BalanceStr { get { return Balance.ToString(LocalSettings.ReportCurrencyFormat); } }
    }
}
