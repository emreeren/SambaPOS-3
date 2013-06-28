using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocument : EntityClass
    {
        public AccountTransactionDocument()
        {
            _accountTransactions = new List<AccountTransaction>();
            Date = DateTime.Now;
        }

        public DateTime Date { get; set; }
        public int DocumentTypeId { get; set; }

        private IList<AccountTransaction> _accountTransactions;
        public virtual IList<AccountTransaction> AccountTransactions
        {
            get { return _accountTransactions; }
            set { _accountTransactions = value; }
        }

        public AccountTransaction AddNewTransaction(AccountTransactionType template, IEnumerable<AccountData> accountDataList)
        {
            var transaction = AccountTransaction.Create(template, accountDataList);
            AccountTransactions.Add(transaction);
            return transaction;
        }

        public AccountTransaction AddNewTransaction(AccountTransactionType template, IEnumerable<AccountData> accountDataList, decimal amount, decimal exchangeRate)
        {
            var transaction = AccountTransaction.Create(template, accountDataList);
            transaction.UpdateAmount(amount, exchangeRate);
            AccountTransactions.Add(transaction);
            return transaction;
        }

        public void AddSingletonTransaction(int transactionTypeId, AccountTransactionType template, IEnumerable<AccountData> accountDataList)
        {
            if (AccountTransactions.SingleOrDefault(x => x.AccountTransactionTypeId == transactionTypeId) == null)
            {
                AddNewTransaction(template, accountDataList);
            }
        }

        public void UpdateSingletonTransactionAmount(int transactionTypeId, string transactionName, decimal amount, decimal exchangeRate)
        {
            var t = AccountTransactions.SingleOrDefault(x => x.AccountTransactionTypeId == transactionTypeId);
            if (t != null)
            {
                t.Name = transactionName;
                t.UpdateAmount(amount, exchangeRate);
            }
        }

        public decimal GetAmount()
        {
            return AccountTransactions.Sum(x => x.Amount);
        }
    }
}
