using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountScreenViewModel : EntityViewModelBase<AccountScreen>
    {
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountScreenViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;

            AddAccountTemplateNameCommand = new CaptionCommand<string>(Resources.Add, OnAddAccountTemplateName);
            DeleteAccountTemplateNameCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteAccountTemplate, CanDeleteAccountTemplate);
        }

        private bool CanDeleteAccountTemplate(string arg)
        {
            return SelectedAccountTemplateName != null;
        }

        private void OnDeleteAccountTemplate(string obj)
        {
            AccountTemplateNamesList.Remove(SelectedAccountTemplateName);
        }

        private void OnAddAccountTemplateName(string obj)
        {
            var acn = new TemplateName(null, AccountTemplateNames);
            AccountTemplateNamesList.Add(acn);
            SelectedAccountTemplateName = acn;
            RaisePropertyChanged(() => SelectedAccountTemplateName);
        }

        public ICaptionCommand AddAccountTemplateNameCommand { get; set; }
        public ICaptionCommand DeleteAccountTemplateNameCommand { get; set; }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = _cacheService.GetAccountTemplates()); }
        }

        private IEnumerable<string> _accountTemplateNames;
        public IEnumerable<string> AccountTemplateNames
        {
            get { return _accountTemplateNames ?? (_accountTemplateNames = AccountTemplates.Select(x => x.Name)); }
        }


        public TemplateName SelectedAccountTemplateName { get; set; }

        private ObservableCollection<TemplateName> _accountTemplateNamesList;
        public ObservableCollection<TemplateName> AccountTemplateNamesList
        {
            get { return _accountTemplateNamesList ?? (_accountTemplateNamesList = new ObservableCollection<TemplateName>((Model.AccountTemplateNames ?? "").Split(';').Select(x => new TemplateName(x, AccountTemplateNames)))); }
        }

        protected override void OnSave(string value)
        {
            Model.AccountTemplateNames = string.Join(";", AccountTemplateNamesList.Where(x => AccountTemplates.Select(y => y.Name).Contains(x.Name)).Select(x => x.Name).Distinct());
            _accountTemplateNamesList = null;
            SelectedAccountTemplateName = null;
            base.OnSave(value);
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

    class TemplateName
    {
        private string _name;
        public string Name
        {
            get { return !string.IsNullOrEmpty(_name) ? _name : string.Format("-{0}-", Resources.Select); }
            set { _name = value; }
        }

        public IEnumerable<string> Names { get; set; }

        public TemplateName(string name, IEnumerable<string> names)
        {
            Name = name;
            Names = names;
        }
    }
}
