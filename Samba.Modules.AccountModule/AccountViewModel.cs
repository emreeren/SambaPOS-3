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

        public AccountTemplate AccountTemplate
        {
            get { return Model.AccountTemplate; }
            set
            {
                Model.AccountTemplate = value;
                _customDataViewModel = null;
                RaisePropertyChanged(() => CustomDataViewModel);
            }
        }

        private AccountCustomDataViewModel _customDataViewModel;
        public AccountCustomDataViewModel CustomDataViewModel
        {
            get { return _customDataViewModel ?? (_customDataViewModel = Model != null ? new AccountCustomDataViewModel(Model) : null); }
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
            RuleFor(x => x.AccountTemplate).NotNull();
        }
    }
}
