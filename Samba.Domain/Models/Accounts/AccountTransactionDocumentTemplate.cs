using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentTemplate : IEntity
    {
        public AccountTransactionDocumentTemplate()
        {
            _transactionTemplates = new List<AccountTransactionTemplate>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ButtonHeader { get; set; }

        private string _buttonColor;
        public string ButtonColor
        {
            get { return _buttonColor ?? "Gainsboro"; }
            set { _buttonColor = value; }
        }

        public int MasterAccountTemplateId { get; set; }

        private readonly IList<AccountTransactionTemplate> _transactionTemplates;
        public virtual IList<AccountTransactionTemplate> TransactionTemplates
        {
            get { return _transactionTemplates; }
        }

        public string DefaultAmount { get; set; }

        public AccountTransactionDocument CreateDocument(Account account, string description, decimal amount)
        {
            Debug.Assert(account.AccountTemplateId == MasterAccountTemplateId);
            var result = new AccountTransactionDocument { Name = Name };
            foreach (var accountTransactionTemplate in TransactionTemplates)
            {
                var transaction = AccountTransaction.Create(accountTransactionTemplate);
                transaction.Name = description;
                transaction.Amount = amount;
                if (transaction.SourceTransactionValue.AccountTemplateId == MasterAccountTemplateId)
                    transaction.SetSoruceAccount(account.Id);
                if (transaction.TargetTransactionValue.AccountTemplateId == MasterAccountTemplateId)
                    transaction.SetTargetAccount(account.Id);
                result.AccountTransactions.Add(transaction);
            }
            return result;
        }
    }
}
