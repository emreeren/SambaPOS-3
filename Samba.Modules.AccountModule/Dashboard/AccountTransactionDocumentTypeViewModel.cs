using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTransactionDocumentTypeViewModel : EntityViewModelBaseWithMap<AccountTransactionDocumentType, AccountTransactionDocumentTypeMap, AbstractMapViewModel<AccountTransactionDocumentTypeMap>>
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountTransactionDocumentTypeViewModel(IAccountService accountService, ICacheService cacheService)
        {
            _accountService = accountService;
            _cacheService = cacheService;
            AddTransactionTypeCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.AccountTransactionType), OnAddTransactionType);
            DeleteTransactionTypeCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.AccountTransactionType), OnDeleteTransactionType, CanDeleteTransactionType);
            AddAccountMapCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.AccountMap), OnAddAccountMap, CanAddAccountMap);
            DeleteAccountMapCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.AccountMap), OnDeleteAccountMap, CanDeleteAccountMap);
        }

        private readonly string[] _filterDescriptions = new[] { Resources.All, Resources.BalancedAccounts, Resources.MappedAccounts };
        public string[] FilterDescriptions
        {
            get { return _filterDescriptions; }
        }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public string DefaultAmount { get { return Model.DefaultAmount; } set { Model.DefaultAmount = value; RaisePropertyChanged(() => DefaultAmount); } }
        public string DescriptionTemplate { get { return Model.DescriptionTemplate; } set { Model.DescriptionTemplate = value; } }
        public string ExchangeTemplate { get { return Model.ExchangeTemplate; } set { Model.ExchangeTemplate = value; } }
        public bool BatchCreateDocuments { get { return Model.BatchCreateDocuments; } set { Model.BatchCreateDocuments = value; RaisePropertyChanged(() => BatchCreateDocuments); } }
        public int Filter { get { return Model.Filter; } set { Model.Filter = value; } } //0 All Accounts , 1 Balanced Accounts
        public string FilterStr { get { return FilterDescriptions[Filter]; } set { Filter = Array.IndexOf(FilterDescriptions, value); } }
        public int? PrinterTemplateId { get { return Model.PrinterTemplateId; } set { Model.PrinterTemplateId = value.GetValueOrDefault(0); } }

        private IEnumerable<string> _defaultAmounts;
        public IEnumerable<string> DefaultAmounts
        {
            get { return _defaultAmounts ?? (_defaultAmounts = GetDefaultAmounts()); }
        }

        public ICaptionCommand AddTransactionTypeCommand { get; set; }
        public ICaptionCommand DeleteTransactionTypeCommand { get; set; }
        public ICaptionCommand AddAccountMapCommand { get; set; }
        public ICaptionCommand DeleteAccountMapCommand { get; set; }

        private AccountType _neededAccountType;
        public AccountType NeededAccountType
        {
            get { return _neededAccountType ?? (_neededAccountType = GetNeededAccountType()); }
        }

        private AccountType GetNeededAccountType()
        {
            var ids = Model.GetNeededAccountTypes();
            if (ids.Any())
            {
                return _cacheService.GetAccountTypeById(ids.First());
            }
            return null;
        }

        public AccountTransactionType SelectedTransactionType { get; set; }
        public AccountTransactionDocumentAccountMapViewModel SelectedAccountMap { get; set; }

        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes ?? (_accountTypes = _cacheService.GetAccountTypes()); }
        }

        private IEnumerable<PrinterTemplate> _printerTemplates;
        public IEnumerable<PrinterTemplate> PrinterTemplates
        {
            get { return _printerTemplates ?? (_printerTemplates = _cacheService.GetPrinterTemplates()); }
        }

        public AccountType MasterAccountType
        {
            get { return AccountTypes.SingleOrDefault(x => x.Id == Model.MasterAccountTypeId); }
            set
            {
                _defaultAmounts = null;
                Model.MasterAccountTypeId = value.Id;
                RaisePropertyChanged(() => MasterAccountType);
                RaisePropertyChanged(() => DefaultAmounts);
            }
        }

        private ObservableCollection<AccountTransactionType> _transactionTypes;
        public ObservableCollection<AccountTransactionType> TransactionTypes
        {
            get { return _transactionTypes ?? (_transactionTypes = new ObservableCollection<AccountTransactionType>(Model.TransactionTypes)); }
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
                        _accountService, x, MasterAccountType, NeededAccountType)));
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
            return Model != null && Model.MasterAccountTypeId > 0;
        }

        private void OnAddAccountMap(string obj)
        {
            var result = new AccountTransactionDocumentAccountMap();
            Model.AccountTransactionDocumentAccountMaps.Add(result);
            AccountTransactionDocumentAccountMaps.Add(new AccountTransactionDocumentAccountMapViewModel(_accountService, result, MasterAccountType, NeededAccountType));
        }

        private bool CanDeleteTransactionType(string arg)
        {
            return SelectedTransactionType != null;
        }

        private void OnDeleteTransactionType(string obj)
        {
            Model.TransactionTypes.Remove(SelectedTransactionType);
            TransactionTypes.Remove(SelectedTransactionType);
        }

        private void OnAddTransactionType(string obj)
        {
            var selectedValues =
                InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<AccountTransactionType>().ToList<IOrderable>(),
                Model.TransactionTypes.ToList<IOrderable>(), Resources.AccountTransactionType.ToPlural(),
                string.Format(Resources.SelectItemsFor_f, Resources.AccountTransactionType.ToPlural(), Model.Name, Resources.DocumentType),
                Resources.AccountTransactionType, Resources.AccountTransactionType.ToPlural());

            foreach (AccountTransactionType selectedValue in selectedValues)
            {
                if (!Model.TransactionTypes.Contains(selectedValue))
                    Model.TransactionTypes.Add(selectedValue);
            }

            _transactionTypes = null;
            RaisePropertyChanged(() => TransactionTypes);
        }

        private IEnumerable<string> GetDefaultAmounts()
        {
            var result = new List<string> { string.Format("[{0}]", Resources.Balance) };
            //if (MasterAccountType != null)
            //{
            //    result.AddRange(MasterAccountType.ResoruceCustomFields.Select(x => string.Format("[:{0}]", x.Name)));
            //}
            return result;
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionDocumentTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.DocumentType;
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<AccountTransactionDocumentTypeMap, AbstractMapViewModel<AccountTransactionDocumentTypeMap>>(Model.AccountTransactionDocumentTypeMaps, Workspace);
        }
    }
}
