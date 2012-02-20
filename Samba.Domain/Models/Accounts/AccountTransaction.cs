using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCalc;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransaction : Entity
    {
        //private string _name;
        //public string Name
        //{
        //    get { return _name; }
        //    set
        //    {
        //        _name = value;
        //        if (SourceTransactionValue != null)
        //            SourceTransactionValue.Name = value;
        //        if (TargetTransactionValue != null)
        //            TargetTransactionValue.Name = value;
        //    }
        //}

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                if (SourceTransactionValue != null)
                    SourceTransactionValue.Credit = value;
                if (TargetTransactionValue != null)
                    TargetTransactionValue.Debit = value;
            }
        }

        public int AccountTransactionTemplateId { get; set; }
        public int AccountTransactionDocumentId { get; set; }
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
                                 AccountTransactionTemplateId = template.Id,
                                 SourceTransactionValue = new AccountTransactionValue(),
                                 TargetTransactionValue = new AccountTransactionValue(),
                             };
            result.Name = template.Name;
            result.SetSoruceAccount(template.DefaultSourceAccountId);
            result.SetTargetAccount(template.DefaultTargetAccountId);
            result.SourceTransactionValue.AccountTemplateId = template.SourceAccountTemplateId;
            result.TargetTransactionValue.AccountTemplateId = template.TargetAccountTemplateId;
            return result;
        }

        public void SetSoruceAccount(int accountId)
        {
            SourceTransactionValue.AccountId = accountId;
        }

        public void SetTargetAccount(int accountId)
        {
            TargetTransactionValue.AccountId = accountId;
        }

        public void UpdateAccounts(int targetAccountTemplateId, int accountId)
        {
            if (SourceTransactionValue.AccountTemplateId == targetAccountTemplateId)
                SetSoruceAccount(accountId);
            if (TargetTransactionValue.AccountTemplateId == targetAccountTemplateId)
                SetTargetAccount(accountId);
        }
    }
}
