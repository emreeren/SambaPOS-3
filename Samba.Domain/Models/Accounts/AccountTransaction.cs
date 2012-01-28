using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransaction : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                SourceTransactionValue.Receivable = value;
                TargetTransactionValue.Liability = value;
            }
        }

        public int AccountTransactionDocumentId { get; set; }

        public int AccountTransactionTemplateId { get; set; }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
        
        private AccountTransactionValue _sourceTransactionValue;
        public virtual AccountTransactionValue SourceTransactionValue
        {
            get { return _sourceTransactionValue ?? (_sourceTransactionValue = new AccountTransactionValue()); }
            set { _sourceTransactionValue = value; }
        }

        private AccountTransactionValue _targetTransactionValue;
        public virtual AccountTransactionValue TargetTransactionValue
        {
            get { return _targetTransactionValue ?? (_targetTransactionValue = new AccountTransactionValue()); }
            set { _targetTransactionValue = value; }
        }

        public static AccountTransaction Create(AccountTransactionTemplate template)
        {
            var result = new AccountTransaction
                             {
                                 AccountTransactionTemplate = template,
                                 SourceTransactionValue = { Account = template.DefaultSourceAccount },
                                 TargetTransactionValue = { Account = template.DefaultTargetAccount }
                             };
            return result;
        }
    }
}
