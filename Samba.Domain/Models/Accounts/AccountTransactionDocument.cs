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

        public AccountTransaction AddNewTransaction(AccountTransactionTemplate template, int accountTemplateId, int accountId)
        {
            var transaction = AccountTransaction.Create(template, accountTemplateId, accountId);
            AccountTransactions.Add(transaction);
            return transaction;
        }

        public AccountTransaction AddNewTransaction(AccountTransactionTemplate template, int accountTemplateId, int accountId, Account account, decimal amount, decimal exchangeRate)
        {
            var transaction = AccountTransaction.Create(template, accountTemplateId, accountId);
            transaction.UpdateAccounts(account.AccountTemplateId, account.Id);
            transaction.UpdateAmount(amount, exchangeRate);
            AccountTransactions.Add(transaction);
            return transaction;
        }

        public void AddSingletonTransaction(int transactionTemplateId, AccountTransactionTemplate template, int accountTemplateId, int accountId)
        {
            if (AccountTransactions.SingleOrDefault(x => x.AccountTransactionTemplateId == transactionTemplateId) == null)
            {
                AddNewTransaction(template, accountTemplateId, accountId);
            }
        }

        public void UpdateSingletonTransactionAmount(int transactionTemplateId, string transactionName, decimal amount, decimal exchangeRate)
        {
            var t = AccountTransactions.SingleOrDefault(x => x.AccountTransactionTemplateId == transactionTemplateId);
            if (t != null)
            {
                t.Name = transactionName;
                t.UpdateAmount(amount, exchangeRate);
            }
        }
    }
}
