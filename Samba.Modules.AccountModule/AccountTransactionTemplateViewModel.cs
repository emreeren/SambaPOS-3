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

        public AccountTemplate SourceAccountTemplate { get { return Model.SourceAccountTemplate; } set { Model.SourceAccountTemplate = value; } }
        public AccountTemplate TargetAccountTemplate { get { return Model.TargetAccountTemplate; } set { Model.TargetAccountTemplate = value; } }
        public AccountTemplate TransactionAccountTemplate { get { return Model.TransactionAccountTemplate; } set { Model.TransactionAccountTemplate = value; } }

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
            RuleFor(x => x.TransactionAccountTemplate).NotNull();
        }
    }
}
