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
    class AccountTransactionDocumentTemplateViewModel : EntityViewModelBaseWithMap<AccountTransactionDocumentTemplate, AccountTransactionDocumentTemplateMap, AbstractMapViewModel<AccountTransactionDocumentTemplateMap>>
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountTransactionDocumentTemplateViewModel(IAccountService accountService, ICacheService cacheService)
        {
            _accountService = accountService;
            _cacheService = cacheService;
            AddTransactionTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.AccountTransactionTemplate), OnAddTransactionTemplate);
            DeleteTransactionTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.AccountTransactionTemplate), OnDeleteTransactionTemplate, CanDeleteTransactionTemplate);
            AddAccountMapCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.AccountMap), OnAddAccountMap, CanAddAccountMap);
            DeleteAccountMapCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.AccountMap), OnDeleteAccountMap, CanDeleteAccountMap);
        }

        private readonly string[] _filterDescriptions = new[] { Resources.All, Resources.BalancedAccounts ,Resources.MappedAccounts};
        public string[] FilterDescriptions
        {
            get { return _filterDescriptions; }
        }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public string DefaultAmount { get { return Model.DefaultAmount; } set { Model.DefaultAmount = value; RaisePropertyChanged(() => DefaultAmount); } }
        public string DescriptionTemplate { get { return Model.DescriptionTemplate; } set { Model.DescriptionTemplate = value; } }
        public bool BatchCreateDocuments { get { return Model.BatchCreateDocuments; } set { Model.BatchCreateDocuments = value; RaisePropertyChanged(() => BatchCreateDocuments); } }
        public int Filter { get { return Model.Filter; } set { Model.Filter = value; } } //0 All Accounts , 1 Balanced Accounts
        public string FilterStr { get { return FilterDescriptions[Filter]; } set { Filter = Array.IndexOf(FilterDescriptions, value); } }

        private IEnumerable<string> _defaultAmounts;
        public IEnumerable<string> DefaultAmounts
        {
            get { return _defaultAmounts ?? (_defaultAmounts = GetDefaultAmounts()); }
        }

        public ICaptionCommand AddTransactionTemplateCommand { get; set; }
        public ICaptionCommand DeleteTransactionTemplateCommand { get; set; }
        public ICaptionCommand AddAccountMapCommand { get; set; }
        public ICaptionCommand DeleteAccountMapCommand { get; set; }

        private AccountTemplate _neededAccountTemplate;
        public AccountTemplate NeededAccountTemplate
        {
            get { return _neededAccountTemplate ?? (_neededAccountTemplate = GetNeededAccountTemplate()); }
        }

        private AccountTemplate GetNeededAccountTemplate()
        {
            var ids = Model.GetNeededAccountTemplates();
            if (ids.Any())
            {
                return _cacheService.GetAccountTemplateById(ids.First());
            }
            return null;
        }

        public AccountTransactionTemplate SelectedTransactionTemplate { get; set; }
        public AccountTransactionDocumentAccountMapViewModel SelectedAccountMap { get; set; }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); }
        }

        public AccountTemplate MasterAccountTemplate
        {
            get { return AccountTemplates.SingleOrDefault(x => x.Id == Model.MasterAccountTemplateId); }
            set
            {
                _defaultAmounts = null;
                Model.MasterAccountTemplateId = value.Id;
                RaisePropertyChanged(() => MasterAccountTemplate);
                RaisePropertyChanged(() => DefaultAmounts);
            }
        }

        private ObservableCollection<AccountTransactionTemplate> _transactionTemplates;
        public ObservableCollection<AccountTransactionTemplate> TransactionTemplates
        {
            get { return _transactionTemplates ?? (_transactionTemplates = new ObservableCollection<AccountTransactionTemplate>(Model.TransactionTemplates)); }
        }

        private ObservableCollection<AccountTransactionDocumentAccountMapViewModel> _accountTransactionDocumentAccountMaps;
        public ObservableCollection<AccountTransactionDocumentAccountMapViewModel> AccountTransactionDocumentAccountMaps
        {
            get { return _accountTransactionDocumentAccountMaps ?? (_accountTransactionDocumentAccountMaps = CreateAccountMaps()); }
        }

        private ObservableCollection<AccountTransactionDocumentAccountMapViewModel> CreateAccountMaps()
        {
            return new ObservableCollection<AccountTransactionDocumentAccountMapViewModel>(
                 Model.AccountTransactionDocumentAccountMaps
                 .Select(x => new AccountTransactionDocumentAccountMapViewModel(
                        _accountService, x, MasterAccountTemplate, NeededAccountTemplate)));
        }

        private bool CanDeleteAccountMap(string arg)
        {
            return SelectedAccountMap != null;
        }

        private void OnDeleteAccountMap(string obj)
        {
            Model.AccountTransactionDocumentAccountMaps.Remove(SelectedAccountMap.Model);
            if (SelectedAccountMap.Model.Id > 0)
                Workspace.Delete(SelectedAccountMap.Model);
            AccountTransactionDocumentAccountMaps.Remove(SelectedAccountMap);
        }

        private bool CanAddAccountMap(string arg)
        {
            return MasterAccountTemplate != null;
        }

        private void OnAddAccountMap(string obj)
        {
            var result = new AccountTransactionDocumentAccountMap();
            Model.AccountTransactionDocumentAccountMaps.Add(result);
            AccountTransactionDocumentAccountMaps.Add(new AccountTransactionDocumentAccountMapViewModel(_accountService, result, MasterAccountTemplate, NeededAccountTemplate));
        }

        private bool CanDeleteTransactionTemplate(string arg)
        {
            return SelectedTransactionTemplate != null;
        }

        private void OnDeleteTransactionTemplate(string obj)
        {
            Model.TransactionTemplates.Remove(SelectedTransactionTemplate);
            TransactionTemplates.Remove(SelectedTransactionTemplate);
        }

        private void OnAddTransactionTemplate(string obj)
        {
            var selectedValues =
                InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<AccountTransactionTemplate>().ToList<IOrderable>(),
                Model.TransactionTemplates.ToList<IOrderable>(), Resources.TicketTags, string.Format(Resources.ChooseTagsForDepartmentHint, Model.Name),
                Resources.TicketTag, Resources.TicketTags);

            foreach (AccountTransactionTemplate selectedValue in selectedValues)
            {
                if (!Model.TransactionTemplates.Contains(selectedValue))
                    Model.TransactionTemplates.Add(selectedValue);
            }

            _transactionTemplates = null;
            RaisePropertyChanged(() => TransactionTemplates);
        }

        private IEnumerable<string> GetDefaultAmounts()
        {
            var result = new List<string> { string.Format("[{0}]", Resources.Balance) };
            //if (MasterAccountTemplate != null)
            //{
            //    result.AddRange(MasterAccountTemplate.ResoruceCustomFields.Select(x => string.Format("[:{0}]", x.Name)));
            //}
            return result;
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionDocumentTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.DocumentTemplate;
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<AccountTransactionDocumentTemplateMap, AbstractMapViewModel<AccountTransactionDocumentTemplateMap>>(Model.AccountTransactionDocumentTemplateMaps, Workspace);
        }
    }
}
