using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.AccountModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountViewModel : EntityViewModelBase<Account>
    {
        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); }
        }

        private AccountTemplate _accountTemplate;
        public AccountTemplate AccountTemplate
        {
            get
            {
                return _accountTemplate ??
                       (_accountTemplate = Workspace.Single<AccountTemplate>(x => x.Id == Model.AccountTemplateId));
            }
            set
            {
                Model.AccountTemplateId = value.Id;
                _accountTemplate = null;
                _customDataViewModel = null;
                RaisePropertyChanged(() => CustomDataViewModel);
                RaisePropertyChanged(() => AccountTemplate);
            }
        }

        private AccountCustomDataViewModel _customDataViewModel;
        public AccountCustomDataViewModel CustomDataViewModel
        {
            get { return _customDataViewModel ?? (_customDataViewModel = Model != null ? new AccountCustomDataViewModel(Model, AccountTemplate) : null); }
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

        protected override void OnSave(string value)
        {
            CustomDataViewModel.Update();
            base.OnSave(value);
        }
    }

    internal class AccountValidator : EntityValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.AccountTemplateId).GreaterThan(0);
        }
    }
}
