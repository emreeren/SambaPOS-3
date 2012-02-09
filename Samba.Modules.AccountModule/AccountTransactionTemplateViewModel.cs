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
    class AccountTransactionTemplateViewModel : EntityViewModelBase<AccountTransactionTemplate>
    {
        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates { get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); } }

        public AccountTemplate SourceAccountTemplate
        {
            get { return Model.SourceAccountTemplate; }
            set
            {
                if (Model.SourceAccountTemplate != value)
                {
                    Model.SourceAccountTemplate = value;
                    _sourceAccounts = null;
                    RaisePropertyChanged("SourceAccounts");
                }
            }
        }
        public AccountTemplate TargetAccountTemplate
        {
            get { return Model.TargetAccountTemplate; }
            set
            {
                if (Model.TargetAccountTemplate != value)
                {
                    Model.TargetAccountTemplate = value;
                    _targetAccounts = null;
                    RaisePropertyChanged("TargetAccounts");
                }
            }
        }

        public string Function { get { return Model.Function; } set { Model.Function = value; } }

        public Account DefaultSourceAccount { get { return Model.DefaultSourceAccount; } set { Model.DefaultSourceAccount = value; } }
        public Account DefaultTargetAccount { get { return Model.DefaultTargetAccount; } set { Model.DefaultTargetAccount = value; } }

        private IEnumerable<Account> _sourceAccounts;
        public IEnumerable<Account> SourceAccounts
        {
            get { return _sourceAccounts ?? (_sourceAccounts = GetSoruceAccounts()); }
        }

        private IEnumerable<Account> _targetAccounts;
        public IEnumerable<Account> TargetAccounts
        {
            get { return _targetAccounts ?? (_targetAccounts = GetTargetAccounts()); }
        }

        private IEnumerable<Account> GetSoruceAccounts()
        {
            return SourceAccountTemplate != null ? Workspace.All<Account>(x => x.AccountTemplate.Id == SourceAccountTemplate.Id).ToList() : null;
        }

        private IEnumerable<Account> GetTargetAccounts()
        {
            return TargetAccountTemplate != null ? Workspace.All<Account>(x => x.AccountTemplate.Id == TargetAccountTemplate.Id).ToList() : null;
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionTemplateView);
        }

        public override string GetModelTypeString()
        {
            return "Account Transaction Template";
        }

        protected override AbstractValidator<AccountTransactionTemplate> GetValidator()
        {
            return new AccountTransactionTemplateValidator();
        }
    }

    internal class AccountTransactionTemplateValidator : EntityValidator<AccountTransactionTemplate>
    {
        public AccountTransactionTemplateValidator()
        {
            RuleFor(x => x.SourceAccountTemplate).NotNull();
            RuleFor(x => x.TargetAccountTemplate).NotNull();
        }
    }
}
