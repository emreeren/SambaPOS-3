using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransaction : EntityClass
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

        private decimal _exchangeRate;
        public decimal ExchangeRate
        {
            get { return _exchangeRate; }
            set
            {
                _exchangeRate = value;
                if (SourceTransactionValue != null)
                {
                    SourceTransactionValue.UpdateExchange(_exchangeRate);
                }
                if (TargetTransactionValue != null)
                {
                    TargetTransactionValue.UpdateExchange(_exchangeRate);
                }
            }
        }

        public int AccountTransactionDocumentId { get; set; }
        public int AccountTransactionTypeId { get; set; }
        public int SourceAccountTypeId { get; set; }
        public int TargetAccountTypeId { get; set; }
        public bool IsReversed { get; set; }
        public bool Reversable { get; set; }

        public decimal Balance
        {
            get
            {
                if (SourceTransactionValue == null || TargetTransactionValue == null) return 0;

                return (SourceTransactionValue.Debit - SourceTransactionValue.Credit) +
                       (TargetTransactionValue.Debit - TargetTransactionValue.Credit);
            }
        }

        private IList<AccountTransactionValue> _accountTransactionValues;
        public virtual IList<AccountTransactionValue> AccountTransactionValues
        {
            get { return _accountTransactionValues; }
            set { _accountTransactionValues = value; }
        }

        public AccountTransactionValue SourceTransactionValue
        {
            get { return AccountTransactionValues.SingleOrDefault(x => x.AccountTypeId == SourceAccountTypeId); }
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
            get { return AccountTransactionValues.SingleOrDefault(x => x.AccountTypeId == TargetAccountTypeId); }
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

        public static AccountTransaction Create(AccountTransactionType template)
        {
            // <pex>
            if (template == null)
                throw new ArgumentNullException("template");
            // </pex>

            var result = new AccountTransaction
                             {
                                 Name = template.Name,
                                 AccountTransactionTypeId = template.Id,
                                 SourceTransactionValue = new AccountTransactionValue { AccountId = template.DefaultSourceAccountId, AccountTypeId = template.SourceAccountTypeId, Name = template.Name },
                                 TargetTransactionValue = new AccountTransactionValue { AccountId = template.DefaultTargetAccountId, AccountTypeId = template.TargetAccountTypeId, Name = template.Name },
                                 SourceAccountTypeId = template.SourceAccountTypeId,
                                 TargetAccountTypeId = template.TargetAccountTypeId
                             };


            return result;
        }

        public static AccountTransaction Create(AccountTransactionType template, IEnumerable<AccountData> accountDataList)
        {
            var result = Create(template);
            result.UpdateAccounts(accountDataList);
            return result;
        }

        public void SetSourceAccount(int accountTypeId, int accountId)
        {
            Debug.Assert(SourceAccountTypeId == accountTypeId);
            SourceTransactionValue.AccountId = accountId;
        }

        public void SetTargetAccount(int accountTypeId, int accountId)
        {
            Debug.Assert(TargetAccountTypeId == accountTypeId);
            TargetTransactionValue.AccountId = accountId;
        }

        public void UpdateAccount(int accountTypeId, int accountId)
        {
            if (SourceAccountTypeId == accountTypeId)
                SourceTransactionValue.AccountId = accountId;
            else if (TargetAccountTypeId == accountTypeId)
                TargetTransactionValue.AccountId = accountId;
        }

        public void UpdateAccounts(IEnumerable<AccountData> accountDataList)
        {
            foreach (var ad in accountDataList)
            {
                UpdateAccount(ad.AccountTypeId, ad.AccountId);
            }
        }

        public void Reverse()
        {
            ReverseTransaction(this);
        }

        private static void ReverseTransaction(AccountTransaction transaction)
        {
            var ti = transaction.SourceAccountTypeId;
            var tv = transaction.SourceTransactionValue;
            transaction.SourceAccountTypeId = transaction.TargetAccountTypeId;
            transaction.SourceTransactionValue = transaction.TargetTransactionValue;
            transaction.TargetAccountTypeId = ti;
            transaction.TargetTransactionValue = tv;
            transaction.IsReversed = true;
        }

        public void UpdateAmount(decimal amount, decimal exchangeRate, IList<AccountData> accounts = null)
        {
            if (amount < 0 && CanReverse())
                Reverse();
            else if (IsReversed && Amount >= 0)
            {
                Reverse();
                IsReversed = false;
            }
            Amount = Math.Abs(amount);
            Amount = Decimal.Round(Amount, 2, MidpointRounding.AwayFromZero);
            ExchangeRate = exchangeRate;
            if (accounts != null)
            {
                var sourceAccount = accounts.FirstOrDefault(x => x.AccountId == SourceTransactionValue.AccountId);
                if (sourceAccount != null)
                    SourceTransactionValue.UpdateExchange(sourceAccount.ExchangeRate);

                var targetAccount = accounts.FirstOrDefault(x => x.AccountId == TargetTransactionValue.AccountId);
                if (targetAccount != null)
                    TargetTransactionValue.UpdateExchange(targetAccount.ExchangeRate);
            }
        }

        private bool CanReverse()
        {
            return !IsReversed && Reversable;
        }

        public bool ContainsAccountId(int accountId)
        {
            return SourceTransactionValue.AccountId == accountId || TargetTransactionValue.AccountId == accountId;
        }

        public void UpdateDescription(string description)
        {
            Name = description;
            TargetTransactionValue.Name = description;
            SourceTransactionValue.Name = description;
        }
    }
}
