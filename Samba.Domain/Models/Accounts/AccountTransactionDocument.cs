using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocument : Entity
    {
        public AccountTransactionDocument()
        {
            _accountTransactions = new List<AccountTransaction>();
            Date = DateTime.Now;
        }

        public DateTime Date { get; set; }

        private readonly IList<AccountTransaction> _accountTransactions;
        public virtual IList<AccountTransaction> AccountTransactions
        {
            get { return _accountTransactions; }
        }

        public AccountTransaction AddNewTransaction(AccountTransactionType template, int accountTypeId, int accountId)
        {
            var transaction = AccountTransaction.Create(template, accountTypeId, accountId);
            AccountTransactions.Add(transaction);
            return transaction;
        }

        public AccountTransaction AddNewTransaction(AccountTransactionType template, int accountTypeId, int accountId, Account account, decimal amount, decimal exchangeRate)
        {
            var transaction = AccountTransaction.Create(template, accountTypeId, accountId);
            transaction.UpdateAccounts(account.AccountTypeId, account.Id);
            transaction.UpdateAmount(amount, exchangeRate);
            AccountTransactions.Add(transaction);
            return transaction;
        }

        public void AddSingletonTransaction(int transactionTypeId, AccountTransactionType template, int accountTypeId, int accountId)
        {
            if (AccountTransactions.SingleOrDefault(x => x.AccountTransactionTypeId == transactionTypeId) == null)
            {
                AddNewTransaction(template, accountTypeId, accountId);
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
