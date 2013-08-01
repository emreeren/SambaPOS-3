using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionValue : EntityClass
    {
        public AccountTransactionValue()
        {
            Date = DateTime.Now;
        }

        public int AccountTypeId { get; set; }
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Exchange { get; set; }
        public int AccountTransactionId { get; set; }
        public int AccountTransactionDocumentId { get; set; }

        public void UpdateExchange(decimal exchangeRate)
        {
            Exchange = exchangeRate == 0 ? 0 : decimal.Round((Debit - Credit) / exchangeRate, 2);
        }
    }
}