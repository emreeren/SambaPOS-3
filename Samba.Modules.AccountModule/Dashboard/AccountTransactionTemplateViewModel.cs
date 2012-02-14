using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTransactionTemplateViewModel : EntityViewModelBase<AccountTransactionTemplate>
    {
        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates { get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); } }

        public AccountTemplate SourceAccountTemplate
        {
            get { return AccountTemplates.SingleOrDefault(x => x.Id == Model.SourceAccountTemplateId); }
            set
            {
                if (Model.SourceAccountTemplateId != value.Id)
                {
                    Model.SourceAccountTemplateId = value.Id;
                    _sourceAccounts = null;
                    RaisePropertyChanged(() => SourceAccountTemplate);
                    RaisePropertyChanged(() => SourceAccounts);
                }
            }
        }
        public AccountTemplate TargetAccountTemplate
        {
            get { return AccountTemplates.SingleOrDefault(x => x.Id == Model.TargetAccountTemplateId); }
            set
            {
                if (Model.TargetAccountTemplateId != value.Id)
                {
                    Model.TargetAccountTemplateId = value.Id;
                    _targetAccounts = null;
                    RaisePropertyChanged(() => TargetAccountTemplate);
                    RaisePropertyChanged(() => TargetAccounts);
                }
            }
        }

        public int DefaultSourceAccountId { get { return Model.DefaultSourceAccountId; } set { Model.DefaultSourceAccountId = value; } }
        public int DefaultTargetAccountId { get { return Model.DefaultTargetAccountId; } set { Model.DefaultTargetAccountId = value; } }

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
            return SourceAccountTemplate != null ? Workspace.All<Account>(x => x.AccountTemplateId == SourceAccountTemplate.Id).ToList() : null;
        }

        private IEnumerable<Account> GetTargetAccounts()
        {
            return TargetAccountTemplate != null ? Workspace.All<Account>(x => x.AccountTemplateId == TargetAccountTemplate.Id).ToList() : null;
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
            RuleFor(x => x.SourceAccountTemplateId).GreaterThan(0);
            RuleFor(x => x.TargetAccountTemplateId).GreaterThan(0);
        }
    }
}
