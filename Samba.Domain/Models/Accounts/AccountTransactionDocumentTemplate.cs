using System.Collections.Generic;
using System.Diagnostics;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentTemplate : Entity, IOrderable
    {
        public AccountTransactionDocumentTemplate()
        {
            _transactionTemplates = new List<AccountTransactionTemplate>();
            _accountTransactionDocumentTemplateMaps = new List<AccountTransactionDocumentTemplateMap>();
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

        private IList<AccountTransactionDocumentTemplateMap> _accountTransactionDocumentTemplateMaps;
        public virtual IList<AccountTransactionDocumentTemplateMap> AccountTransactionDocumentTemplateMaps
        {
            get { return _accountTransactionDocumentTemplateMaps; }
            set { _accountTransactionDocumentTemplateMaps = value; }
        }

        public string DefaultAmount { get; set; }
        public string DescriptionTemplate { get; set; }
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

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
                result.AccountTransactions.Add(transaction);
            }
            return result;
        }


        public void AddAccountTransactionDocumentTemplateMap()
        {
            AccountTransactionDocumentTemplateMaps.Add(new AccountTransactionDocumentTemplateMap());
        }
    }
}
