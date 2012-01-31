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

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (SourceTransactionValue != null)
                    SourceTransactionValue.Name = value;
                if (TargetTransactionValue != null)
                    TargetTransactionValue.Name = value;
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                if (SourceTransactionValue != null)
                    SourceTransactionValue.Receivable = value;
                if (TargetTransactionValue != null)
                    TargetTransactionValue.Liability = value;
            }
        }

        public int AccountTransactionDocumentId { get; set; }
        public int AccountTransactionTemplateId { get; set; }

        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
        public virtual AccountTransactionValue SourceTransactionValue { get; set; }
        public virtual AccountTransactionValue TargetTransactionValue { get; set; }

        private static AccountTransaction _null;
        public static AccountTransaction Null
        {
            get
            {
                return _null ?? (_null = new AccountTransaction
                                             {
                                                 SourceTransactionValue = new AccountTransactionValue(),
                                                 TargetTransactionValue = new AccountTransactionValue()
                                             });
            }
        }

        public static AccountTransaction Create(AccountTransactionTemplate template)
        {
            var result = new AccountTransaction
                             {
                                 AccountTransactionTemplate = template,
                                 SourceTransactionValue = new AccountTransactionValue(),
                                 TargetTransactionValue = new AccountTransactionValue()
                             };
            result.SourceTransactionValue.Account = template.DefaultSourceAccount;
            result.TargetTransactionValue.Account = template.DefaultTargetAccount;
            return result;
        }
    }
}
