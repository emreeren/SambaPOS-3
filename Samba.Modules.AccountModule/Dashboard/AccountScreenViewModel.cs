using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Omu.ValueInjecter;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.DaoClasses;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountScreenViewModel : EntityViewModelBase<AccountScreen>
    {
        private readonly ICacheDao _dataService;

        [ImportingConstructor]
        public AccountScreenViewModel(ICacheDao dataService)
        {
            _dataService = dataService;
            AddScreenFilterCommand = new CaptionCommand<string>(Resources.Select, OnAddScreenFilter);
        }

        public string[] FilterTypes { get { return new[] { Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod }; } }
        public string FilterType { get { return FilterTypes[Model.Filter]; } set { Model.Filter = FilterTypes.ToList().IndexOf(value); } }

        public CaptionCommand<string> AddScreenFilterCommand { get; set; }

        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes ?? (_accountTypes = _dataService.GetAccountTypes()); }
        }

        private ObservableCollection<AccountScreenValue> _accountScreenFilters;
        public ObservableCollection<AccountScreenValue> AccountScreenFilters
        {
            get { return _accountScreenFilters ?? (_accountScreenFilters = new ObservableCollection<AccountScreenValue>(Model.AccountScreenValues.OrderBy(x => x.Order))); }
        }

        private void OnAddScreenFilter(string obj)
        {
            var selectedItems = Model.AccountScreenValues;
            var values = AccountTypes.Where(x => selectedItems.All(y => y.AccountTypeName != x.Name)).Select(x => new AccountScreenValue { AccountTypeName = x.Name, AccountTypeId = x.Id }).ToList<IOrderable>();

            var selectedValues = InteractionService.UserIntraction.ChooseValuesFrom(
                values,
                selectedItems.ToList<IOrderable>(),
                Resources.AccountType.ToPlural(),
                string.Format(Resources.SelectItemsFor_f, Resources.AccountType.ToPlural(), Model.Name, Resources.AccountScreen),
                Resources.AccountType,
                Resources.AccountType.ToPlural());

            Model.InjectFrom<EntityInjection>(new { AccountScreenValues = selectedValues.Cast<AccountScreenValue>().ToList() });

            _accountScreenFilters = null;
            RaisePropertyChanged(() => AccountScreenFilters);
        }

        public override Type GetViewType()
        {
            return typeof(AccountScreenView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountScreen;
        }
    }

}
