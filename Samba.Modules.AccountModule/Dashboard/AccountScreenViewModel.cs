using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
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
            AddAccountTemplateNameCommand = new CaptionCommand<string>(Resources.Select, OnAddAccountTemplateName);
        }

        public ICaptionCommand AddAccountTemplateNameCommand { get; set; }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = _accountService.GetAccountTemplates()); }
        }

        private ObservableCollection<string> _accountTemplateNamesList;
        public ObservableCollection<string> AccountTemplateNamesList
        {
            get { return _accountTemplateNamesList ?? (_accountTemplateNamesList = new ObservableCollection<string>((Model.AccountTemplateNames ?? "").Split(';'))); }
        }

        protected override void OnSave(string value)
        {
            Model.AccountTemplateNames = string.Join(";", AccountTemplateNamesList.Where(x => AccountTemplates.Select(y => y.Name).Contains(x)).Distinct());
            _accountTemplateNamesList = null;
            _accountTemplates = null;
            base.OnSave(value);
        }

        private void OnAddAccountTemplateName(string obj)
        {
            var selectedItems = AccountTemplates.Where(x => AccountTemplateNamesList.Contains(x.Name)).ToList<IOrderable>();
            var selectedValues =
              InteractionService.UserIntraction.ChooseValuesFrom(AccountTemplates.Where(x => !selectedItems.Contains(x)).ToList<IOrderable>(), selectedItems,
              Resources.AccountTemplate.ToPlural(), string.Format(Resources.SelectItemsFor_f, Resources.AccountTemplate.ToPlural(), Model.Name, Resources.AccountScreen),
              Resources.AccountTemplate, Resources.AccountTemplate.ToPlural());
            AccountTemplateNamesList.Clear();
            AccountTemplateNamesList.AddRange(selectedValues.Select(x => x.Name));
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
