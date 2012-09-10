using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samba.Infrastructure.Data;
using Samba.Domain.Models.Accounts;
using System;

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

        private readonly IList<AccountTransactionDocumentTemplateMap> _accountTransactionDocumentTemplateMaps;
        public virtual IList<AccountTransactionDocumentTemplateMap> AccountTransactionDocumentTemplateMaps
        {
            get { return _accountTransactionDocumentTemplateMaps; }
        }

        public string DefaultAmount { get; set; }
        public string DescriptionTemplate { get; set; }
        public bool BatchCreateDocuments { get; set; }
        public int Filter { get; set; }
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public AccountTransactionDocument CreateDocument(Account account, string description, decimal amount, IList<Account> accounts)
        {
            // <pex>
            if (account == null)
                throw new ArgumentNullException("account");
            if (account.AccountTemplateId != MasterAccountTemplateId)
                throw new ArgumentException("account template should match master account template");
            // </pex>

            var result = new AccountTransactionDocument { Name = Name };
            foreach (var accountTransactionTemplate in TransactionTemplates)
            {
                var transaction = AccountTransaction.Create(accountTransactionTemplate);
                transaction.Name = description;
                transaction.UpdateAmount(amount);
                transaction.UpdateAccounts(MasterAccountTemplateId, account.Id);
                if (accounts != null && accounts.Count > 0)
                {
                    if (transaction.SourceAccountTemplateId != MasterAccountTemplateId &&
                        transaction.SourceTransactionValue.AccountId == 0)
                    {
                        Account ac =
                            accounts.FirstOrDefault(x => x.AccountTemplateId == transaction.SourceAccountTemplateId);
                        if (ac != null) transaction.SetSourceAccount(ac.AccountTemplateId, ac.Id);
                    }
                    if (transaction.TargetAccountTemplateId != MasterAccountTemplateId &&
                        transaction.TargetTransactionValue.AccountId == 0)
                    {
                        Account ac =
                            accounts.FirstOrDefault(x => x.AccountTemplateId == transaction.TargetAccountTemplateId);
                        if (ac != null) transaction.SetTargetAccount(ac.AccountTemplateId, ac.Id);
                    }
                }
                result.AccountTransactions.Add(transaction);
            }
            return result;
        }


        public void AddAccountTransactionDocumentTemplateMap()
        {
            AccountTransactionDocumentTemplateMaps.Add(new AccountTransactionDocumentTemplateMap());
        }

        public List<int> GetNeededAccountTemplates()
        {
            var result = new List<int>();
            foreach (var accountTransactionTemplate in TransactionTemplates)
            {
                if (accountTransactionTemplate.TargetAccountTemplateId != MasterAccountTemplateId &&
                    accountTransactionTemplate.DefaultTargetAccountId == 0)
                {
                    if (!result.Contains(accountTransactionTemplate.TargetAccountTemplateId))
                        result.Add(accountTransactionTemplate.TargetAccountTemplateId);
                }
                if (accountTransactionTemplate.SourceAccountTemplateId != MasterAccountTemplateId &&
                    accountTransactionTemplate.DefaultSourceAccountId == 0)
                {
                    if (!result.Contains(accountTransactionTemplate.SourceAccountTemplateId))
                        result.Add(accountTransactionTemplate.SourceAccountTemplateId);
                }
            }
            return result;
        }
    }
}
