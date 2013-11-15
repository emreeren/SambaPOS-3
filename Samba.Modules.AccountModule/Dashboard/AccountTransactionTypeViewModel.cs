using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTransactionTypeViewModel : EntityViewModelBase<AccountTransactionType>
    {
        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes { get { return _accountTypes ?? (_accountTypes = Workspace.All<AccountType>()); } }

        private IEnumerable<ForeignCurrency> _foreignCurrencies;
        public IEnumerable<ForeignCurrency> ForeignCurrencies
        {
            get { return _foreignCurrencies ?? (_foreignCurrencies = Workspace.All<ForeignCurrency>().ToList()); }
        }

        public ForeignCurrency ForeignCurrency
        {
            get
            {
                return ForeignCurrencies.SingleOrDefault(x => x.Id == Model.ForeignCurrencyId);
            }
            set
            {
                Model.ForeignCurrencyId = value != null ? value.Id : 0;
                RaisePropertyChanged(() => ForeignCurrency);
            }
        }
        public AccountType SourceAccountType
        {
            get { return AccountTypes.SingleOrDefault(x => x.Id == Model.SourceAccountTypeId); }
            set
            {
                Model.SourceAccountTypeId = value != null ? value.Id : 0;
                _sourceAccounts = null;
                RaisePropertyChanged(() => SourceAccountType);
                RaisePropertyChanged(() => SourceAccounts);
            }
        }
        public AccountType TargetAccountType
        {
            get { return AccountTypes.SingleOrDefault(x => x.Id == Model.TargetAccountTypeId); }
            set
            {
                Model.TargetAccountTypeId = value != null ? value.Id : 0;
                _targetAccounts = null;
                RaisePropertyChanged(() => TargetAccountType);
                RaisePropertyChanged(() => TargetAccounts);
            }
        }

        public int? DefaultSourceAccountId { get { return Model.DefaultSourceAccountId; } set { Model.DefaultSourceAccountId = value.GetValueOrDefault(0); } }
        public int? DefaultTargetAccountId { get { return Model.DefaultTargetAccountId; } set { Model.DefaultTargetAccountId = value.GetValueOrDefault(0); } }

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
            return SourceAccountType != null ? Workspace.All<Account>(x => x.AccountTypeId == SourceAccountType.Id).ToList() : null;
        }

        private IEnumerable<Account> GetTargetAccounts()
        {
            return TargetAccountType != null ? Workspace.All<Account>(x => x.AccountTypeId == TargetAccountType.Id).ToList() : null;
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountTransactionType;
        }

        protected override AbstractValidator<AccountTransactionType> GetValidator()
        {
            return new AccountTransactionTypeValidator();
        }
    }

    internal class AccountTransactionTypeValidator : EntityValidator<AccountTransactionType>
    {
        public AccountTransactionTypeValidator()
        {
            RuleFor(x => x.SourceAccountTypeId).GreaterThan(0).When(x => x.TargetAccountTypeId == 0);
            RuleFor(x => x.TargetAccountTypeId).GreaterThan(0).When(x => x.SourceAccountTypeId == 0);
            RuleFor(x => x.SourceAccountTypeId).NotEqual(x => x.TargetAccountTypeId).When(
                x => x.SourceAccountTypeId > 0 && x.TargetAccountTypeId > 0);
        }
    }
}
