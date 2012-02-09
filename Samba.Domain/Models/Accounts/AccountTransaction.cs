using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCalc;
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

        public int AccountTransactionTemplateId { get; set; }
        public int AccountTransactionDocumentId { get; set; }
        public virtual AccountTransactionValue SourceTransactionValue { get; set; }
        public virtual AccountTransactionValue TargetTransactionValue { get; set; }

        public string Function { get; set; }

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

        public static AccountTransaction Create(AccountTransactionTemplate template, AccountTransactionDocument document)
        {
            var result = new AccountTransaction
                             {
                                 Name = template.Name,
                                 AccountTransactionTemplateId = template.Id,
                                 SourceTransactionValue = new AccountTransactionValue(),
                                 TargetTransactionValue = new AccountTransactionValue(),
                                 Function = template.Function
                             };

            result.SetSoruceAccount(template.DefaultSourceAccount, document);
            result.SetTargetAccount(template.DefaultTargetAccount, document);
            return result;
        }

        public void SetSoruceAccount(Account account, AccountTransactionDocument document)
        {
            if (account == null) return;
            SourceTransactionValue.AccountId = account.Id;
            SourceTransactionValue.AccountName = account.Name;
            _calculatingAccount = account;
            _calculatingDocument = document;
            DoCalculations();
        }

        public void SetTargetAccount(Account account, AccountTransactionDocument document)
        {
            if (account == null) return;
            TargetTransactionValue.AccountId = account.Id;
            TargetTransactionValue.AccountName = account.Name;
            _calculatingAccount = account;
            _calculatingDocument = document;
            DoCalculations();
        }

        private void DoCalculations()
        {
            if (!string.IsNullOrEmpty(Function))
            {
                var e = new Expression(Function);
                e.EvaluateParameter += e_EvaluateParameter;
                var result = e.Evaluate();
                e.EvaluateParameter -= e_EvaluateParameter;
            }
        }

        private Account _calculatingAccount;
        private AccountTransactionDocument _calculatingDocument;

        void e_EvaluateParameter(string name, ParameterArgs args)
        {
            args.Result = 1;
        }
    }
}
