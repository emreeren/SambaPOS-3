using System;
using System.Collections.Generic;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    public class AccountViewModel : EntityViewModelBase<Account>
    {
        public AccountViewModel(Account model)
            : base(model)
        {
        }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); }
        }

        public AccountTemplate AccountTemplate
        {
            get { return Model.AccountTemplate; }
            set { Model.AccountTemplate = value; }
        }

        public override Type GetViewType()
        {
            return typeof(AccountView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Account;
        }

        public string SearchString { get { return Model.SearchString; } set { Model.SearchString = value; } }

        protected override AbstractValidator<Account> GetValidator()
        {
            return new AccountValidator();
        }
    }

    internal class AccountValidator : EntityValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.AccountTemplate).NotNull();
        }
    }
}
