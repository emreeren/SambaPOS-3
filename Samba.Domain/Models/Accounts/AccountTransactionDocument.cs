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

        public AccountTransaction AddNewTransaction(AccountTransactionTemplate template, int accountTemplateId, int accountId, Account account, decimal amount)
        {
            var transaction = AccountTransaction.Create(template, accountTemplateId, accountId);
            if (amount < 0)
            {
                //Inverse accounts;
                var ti = transaction.SourceAccountTemplateId;
                var tv = transaction.SourceTransactionValue;
                transaction.SourceAccountTemplateId = transaction.TargetAccountTemplateId;
                transaction.SourceTransactionValue = transaction.TargetTransactionValue;
                transaction.TargetAccountTemplateId = ti;
                transaction.TargetTransactionValue = tv;
                amount = Math.Abs(amount);
            }
            transaction.UpdateAccounts(account.AccountTemplateId, account.Id);
            transaction.Amount = amount;
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

        public void AddSingletonTransaction(int transactionTemplateId, AccountTransactionTemplate template, int accountTemplateId, int accountId, string transactionName,decimal amount)
        {
            if (AccountTransactions.SingleOrDefault(x => x.AccountTransactionTemplateId == transactionTemplateId) == null)
            {
                var transaction = AddNewTransaction(template, accountTemplateId, accountId);
                transaction.Name = transactionName;
                transaction.Amount = amount;
            }
        }
    }
}
