using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common.DataGeneration;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountViewModel : EntityViewModelBase<Account>, IEntityCreator<Account>
    {
        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes ?? (_accountTypes = Workspace.All<AccountType>()); }
        }

        private AccountType _accountType;
        public AccountType AccountType
        {
            get
            {
                return _accountType ??
                       (_accountType = Workspace.Single<AccountType>(x => x.Id == Model.AccountTypeId));
            }
            set
            {
                Model.AccountTypeId = value.Id;
                _accountType = null;
                RaisePropertyChanged(() => AccountType);
            }
        }

        public string GroupValue { get { return NameCache.GetName<AccountType>(Model.AccountTypeId); } }

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

        public override Type GetViewType()
        {
            return typeof(AccountView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Account;
        }

        protected override AbstractValidator<Account> GetValidator()
        {
            return new AccountValidator();
        }

        public IEnumerable<Account> CreateItems(IEnumerable<string> data)
        {
            return new DataCreationService().BatchCreateAccounts(data.ToArray(), Workspace);
        }
    }

    internal class AccountValidator : EntityValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.AccountTypeId).GreaterThan(0);
        }
    }
}
