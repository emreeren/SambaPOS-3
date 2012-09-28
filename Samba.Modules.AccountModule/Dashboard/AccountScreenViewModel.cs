using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Omu.ValueInjecter;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountScreenViewModel : EntityViewModelBase<AccountScreen>
    {
        private readonly IAccountService _accountService;

        [ImportingConstructor]
        public AccountScreenViewModel(IAccountService accountService)
        {
            _accountService = accountService;
            AddScreenFilterCommand = new CaptionCommand<string>(Resources.Select, OnAddScreenFilter);
        }

        public string[] FilterTypes { get { return new[] { Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod }; } }
        public string FilterType { get { return FilterTypes[Model.Filter]; } set { Model.Filter = FilterTypes.ToList().IndexOf(value); } }

        public CaptionCommand<string> AddScreenFilterCommand { get; set; }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = _accountService.GetAccountTemplates()); }
        }


        private ObservableCollection<AccountScreenValue> _accountScreenFilters;
        public ObservableCollection<AccountScreenValue> AccountScreenFilters
        {
            get { return _accountScreenFilters ?? (_accountScreenFilters = new ObservableCollection<AccountScreenValue>(Model.AccountScreenValues.OrderBy(x => x.Order))); }
        }

        private void OnAddScreenFilter(string obj)
        {
            var selectedItems = Model.AccountScreenValues;
            var values = AccountTemplates.Where(x => selectedItems.All(y => y.AccountTemplateName != x.Name)).Select(x => new AccountScreenValue { AccountTemplateName = x.Name, AccountTemplateId = x.Id }).ToList<IOrderable>();

            var selectedValues = InteractionService.UserIntraction.ChooseValuesFrom(
                values,
                selectedItems.ToList<IOrderable>(),
                Resources.AccountTemplate.ToPlural(),
                string.Format(Resources.SelectItemsFor_f, Resources.AccountTemplate.ToPlural(), Model.Name, Resources.AccountScreen),
                Resources.AccountTemplate,
                Resources.AccountTemplate.ToPlural());

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
