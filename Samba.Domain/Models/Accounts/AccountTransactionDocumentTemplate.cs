using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentTemplate : Entity
    {
        public AccountTransactionDocumentTemplate()
        {
            _transactionTemplates = new List<AccountTransactionTemplate>();
        }

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
        public string DescriptionTemplate { get; set; }

        public AccountTransactionDocument CreateDocument(Account account, string description, decimal amount)
        {
            Debug.Assert(account.AccountTemplateId == MasterAccountTemplateId);
            var result = new AccountTransactionDocument { Name = Name };
            foreach (var accountTransactionTemplate in TransactionTemplates)
            {
                var transaction = AccountTransaction.Create(accountTransactionTemplate);
                transaction.Name = description;
                transaction.Amount = amount;
                transaction.UpdateAccounts(MasterAccountTemplateId, account.Id);
                //if (transaction.SourceAccountTemplateId == MasterAccountTemplateId)
                //    transaction.SourceTransactionValue.AccountId = account.Id;
                //if (transaction.TargetAccountTemplateId == MasterAccountTemplateId)
                //    transaction.TargetTransactionValue.AccountId = account.Id;
                result.AccountTransactions.Add(transaction);
            }
            return result;
        }
    }
}
