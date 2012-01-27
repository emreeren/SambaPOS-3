using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTransactionDocumentTemplateViewModel : EntityViewModelBase<AccountTransactionDocumentTemplate>
    {
        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates
        {
            get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = Workspace.All<AccountTransactionTemplate>()).ToList(); }
        }

        private IEnumerable<Account> _sourceAccounts;
        public IEnumerable<Account> SourceAccounts
        {
            get { return _sourceAccounts ?? (_sourceAccounts = GetSourceAccounts()); }
        }

        private IEnumerable<Account> _targetAccounts;
        public IEnumerable<Account> TargetAccounts
        {
            get { return _targetAccounts ?? (_targetAccounts = GetTargetAccounts()); }
        }

        private IEnumerable<Account> _transactionAccounts;
        public IEnumerable<Account> TransactionAccounts
        {
            get { return _transactionAccounts ?? (_transactionAccounts = GetTransactionAccounts()); }
        }

        public AccountTransactionTemplate AccountTransactionTemplate
        {
            get { return Model.AccountTransactionTemplate; }
            set
            {
                Model.AccountTransactionTemplate = value;
                _sourceAccounts = null;
                _targetAccounts = null;
                _transactionAccounts = null;
                RaisePropertyChanged(() => SourceAccounts);
                RaisePropertyChanged(() => TargetAccounts);
                RaisePropertyChanged(() => TransactionAccounts);
            }
        }

        public Account SourceAccount { get { return Model.SourceAccount; } set { Model.SourceAccount = value; } }
        public Account TargetAccount { get { return Model.TargetAccount; } set { Model.TargetAccount = value; } }
        public Account TransactionAccount { get { return Model.TransactionAccount; } set { Model.TransactionAccount = value; } }

        private IEnumerable<Account> GetSourceAccounts()
        {
            if (AccountTransactionTemplate == null || AccountTransactionTemplate.SourceAccountTemplate == null)
                return new List<Account>();
            return Workspace.All<Account>(x => x.AccountTemplate.Id == AccountTransactionTemplate.SourceAccountTemplate.Id).ToList();
        }

        private IEnumerable<Account> GetTargetAccounts()
        {
            if (AccountTransactionTemplate == null || AccountTransactionTemplate.TargetAccountTemplate == null)
                return new List<Account>();
            return Workspace.All<Account>(x => x.AccountTemplate.Id == AccountTransactionTemplate.TargetAccountTemplate.Id).ToList();
        }

        private IEnumerable<Account> GetTransactionAccounts()
        {
            if (AccountTransactionTemplate == null || AccountTransactionTemplate.TransactionAccountTemplate == null)
                return new List<Account>();
            return Workspace.All<Account>(x => x.AccountTemplate.Id == AccountTransactionTemplate.TargetAccountTemplate.Id).ToList();
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionDocumentTemplateView);
        }

        public override string GetModelTypeString()
        {
            return "Account Transaction Document Template";
        }

        protected override AbstractValidator<AccountTransactionDocumentTemplate> GetValidator()
        {
            return new AccountTransactionDocumentTemplateValidator();
        }
    }

    internal class AccountTransactionDocumentTemplateValidator : EntityValidator<AccountTransactionDocumentTemplate>
    {
        public AccountTransactionDocumentTemplateValidator()
        {
            RuleFor(x => x.AccountTransactionTemplate).NotNull();
        }
    }
}
