using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransaction : Entity
    {
        public AccountTransaction()
        {
            _accountTransactionValues = new List<AccountTransactionValue>();
            Reversable = true;
        }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                if (SourceTransactionValue != null)
                {
                    SourceTransactionValue.Debit = 0;
                    SourceTransactionValue.Credit = value;
                }
                if (TargetTransactionValue != null)
                {
                    TargetTransactionValue.Credit = 0;
                    TargetTransactionValue.Debit = value;
                }
            }
        }

        public int AccountTransactionDocumentId { get; set; }
        public int AccountTransactionTemplateId { get; set; }
        public int SourceAccountTemplateId { get; set; }
        public int TargetAccountTemplateId { get; set; }
        public bool IsReversed { get; set; }
        public bool Reversable { get; set; }

        private readonly IList<AccountTransactionValue> _accountTransactionValues;
        public virtual IList<AccountTransactionValue> AccountTransactionValues
        {
            get { return _accountTransactionValues; }
        }

        public AccountTransactionValue SourceTransactionValue
        {
            get { return AccountTransactionValues.SingleOrDefault(x => x.AccountTemplateId == SourceAccountTemplateId); }
            set
            {
                if (SourceTransactionValue != value)
                {
                    if (SourceTransactionValue != null)
                        AccountTransactionValues.Remove(SourceTransactionValue);
                    AccountTransactionValues.Add(value);
                }
            }
        }

        public AccountTransactionValue TargetTransactionValue
        {
            get { return AccountTransactionValues.SingleOrDefault(x => x.AccountTemplateId == TargetAccountTemplateId); }
            set
            {
                if (TargetTransactionValue != value)
                {
                    if (TargetTransactionValue != null)
                        AccountTransactionValues.Remove(TargetTransactionValue);
                    AccountTransactionValues.Add(value);
                }
            }
        }

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
            // <pex>
            if (template == null)
                throw new ArgumentNullException("template");
            // </pex>

            var result = new AccountTransaction
                             {
                                 Name = template.Name,
                                 AccountTransactionTemplateId = template.Id,
                                 SourceTransactionValue = new AccountTransactionValue { AccountId = template.DefaultSourceAccountId, AccountTemplateId = template.SourceAccountTemplateId, Name = template.Name },
                                 TargetTransactionValue = new AccountTransactionValue { AccountId = template.DefaultTargetAccountId, AccountTemplateId = template.TargetAccountTemplateId, Name = template.Name },
                                 SourceAccountTemplateId = template.SourceAccountTemplateId,
                                 TargetAccountTemplateId = template.TargetAccountTemplateId
                             };


            return result;
        }

        public static AccountTransaction Create(AccountTransactionTemplate template, int accountTemplateId, int accountId)
        {
            var result = Create(template);
            result.UpdateAccounts(accountTemplateId, accountId);
            return result;
        }

        public void SetSourceAccount(int accountTemplateId, int accountId)
        {
            Debug.Assert(SourceAccountTemplateId == accountTemplateId);
            SourceTransactionValue.AccountId = accountId;
        }

        public void SetTargetAccount(int accountTemplateId, int accountId)
        {
            Debug.Assert(TargetAccountTemplateId == accountTemplateId);
            TargetTransactionValue.AccountId = accountId;
        }

        public void UpdateAccounts(int accountTemplateId, int accountId)
        {
            if (SourceAccountTemplateId == accountTemplateId)
                SourceTransactionValue.AccountId = accountId;
            else if (TargetAccountTemplateId == accountTemplateId)
                TargetTransactionValue.AccountId = accountId;
        }

        public void Reverse()
        {
            ReverseTransaction(this);
        }

        private static void ReverseTransaction(AccountTransaction transaction)
        {
            var ti = transaction.SourceAccountTemplateId;
            var tv = transaction.SourceTransactionValue;
            transaction.SourceAccountTemplateId = transaction.TargetAccountTemplateId;
            transaction.SourceTransactionValue = transaction.TargetTransactionValue;
            transaction.TargetAccountTemplateId = ti;
            transaction.TargetTransactionValue = tv;
            transaction.IsReversed = true;
        }

        public void UpdateAmount(decimal amount)
        {
            if (amount < 0 && CanReverse())
                Reverse();
            else if (IsReversed && Amount >= 0)
            {
                Reverse();
                IsReversed = false;
            }
            Amount = Math.Abs(amount);
        }

        private bool CanReverse()
        {
            return !IsReversed && Reversable;
        }
    }
}
