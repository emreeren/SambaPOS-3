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

        public int AccountTransactionDocumentId { get; set; }
        public int AccountTransactionTemplateId { get; set; }
        public int SourceAccountTemplateId { get; set; }
        public int TargetAccountTemplateId { get; set; }
        public bool? DynamicPart { get; set; }
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
                                 Name = template.Name,
                                 AccountTransactionTemplateId = template.Id,
                                 SourceTransactionValue = new AccountTransactionValue(),
                                 TargetTransactionValue = new AccountTransactionValue()
                             };

            if (result.SourceTransactionValue != null)
            {
                result.SourceTransactionValue.AccountId = template.DefaultSourceAccountId;
                result.SourceTransactionValue.Name = template.Name;
            }

            if (result.TargetTransactionValue != null)
            {
                result.TargetTransactionValue.AccountId = template.DefaultTargetAccountId;
                result.TargetTransactionValue.Name = template.Name;
            }

            result.SourceAccountTemplateId = template.SourceAccountTemplateId;
            result.TargetAccountTemplateId = template.TargetAccountTemplateId;

            result.DynamicPart = null;
            if (template.DefaultSourceAccountId == 0) result.DynamicPart = true;
            if (template.DefaultTargetAccountId == 0) result.DynamicPart = false;

            return result;
        }

        public void SetSourceAccount(int accountTemplateId, int accountId)
        {
            SourceAccountTemplateId = accountTemplateId;
            SourceTransactionValue.AccountId = accountId;
        }

        public void SetTargetAccount(int accountTemplateId, int accountId)
        {
            TargetAccountTemplateId = accountTemplateId;
            TargetTransactionValue.AccountId = accountId;
        }

        public void UpdateAccounts(int accountTemplateId, int accountId)
        {
            if (SourceAccountTemplateId == accountTemplateId)
                SourceTransactionValue.AccountId = accountId;
            else if (TargetAccountTemplateId == accountTemplateId)
                TargetTransactionValue.AccountId = accountId;
            else if (DynamicPart == true)
                SetSourceAccount(accountTemplateId, accountId);
            else if (DynamicPart == false)
                SetTargetAccount(accountTemplateId, accountId);
        }
    }
}
