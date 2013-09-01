using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTransactionDocumentViewModel : EntityViewModelBase<AccountTransactionDocument>
    {
        private readonly ICacheService _cacheService;
        private readonly IPrinterService _printerService;
        private readonly IApplicationState _applicationState;

        public event EventHandler RowInserted;
        public event EventHandler RowDeleted;

        [ImportingConstructor]
        public AccountTransactionDocumentViewModel(ICacheService cacheService, IPrinterService printerService, IApplicationState applicationState)
        {
            _cacheService = cacheService;
            _printerService = printerService;
            _applicationState = applicationState;
            AddItemCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Line), OnAddItem);
            DeleteItemCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Line), OnDeleteItem,CanDeleteItem);
            PrintCommand = new CaptionCommand<string>(Resources.Print, OnPrint, CanPrint);
        }



        private bool CanPrint(string arg)
        {
            return Model.DocumentTypeId > 0;
        }

        private void OnPrint(string obj)
        {
            var printer = _applicationState.GetTransactionPrinter();
            var printerTemplateId = _cacheService.GetAccountTransactionDocumentTypeById(Model.DocumentTypeId).PrinterTemplateId;
            var printerTemplate = _cacheService.GetPrinterTemplates().First(x => x.Id == printerTemplateId);
            _printerService.PrintObject(Model, printer, printerTemplate);
        }

        public ICaptionCommand AddItemCommand { get; set; }
        public ICaptionCommand DeleteItemCommand { get; set; }
        public ICaptionCommand PrintCommand { get; set; }

        public AccountTransactionViewModel SelectedTransaction
        {
            get { return _selectedTransaction; }
            set { _selectedTransaction = value; RaisePropertyChanged(() => SelectedTransaction); }
        }

        private ObservableCollection<AccountTransactionViewModel> _accountTransactions;
        private AccountTransactionViewModel _selectedTransaction;

        public ObservableCollection<AccountTransactionViewModel> AccountTransactions
        {
            get { return _accountTransactions ?? (_accountTransactions = CreateAccountTransactions()); }
        }

        private ObservableCollection<AccountTransactionViewModel> CreateAccountTransactions()
        {
            var result = new ObservableCollection<AccountTransactionViewModel>();
            result.AddRange(Model.AccountTransactions.Select(x => new AccountTransactionViewModel(Workspace, x, Model)));
            return result;
        }

        private void OnDeleteItem(string obj)
        {
            if (SelectedTransaction.Model.Id > 0)
                Workspace.Delete(SelectedTransaction.Model);
            Model.AccountTransactions.Remove(SelectedTransaction.Model);
            AccountTransactions.Remove(SelectedTransaction);
            OnRowDeleted();
        }

        private bool CanDeleteItem(string arg)
        {
            return SelectedTransaction != null;
        }

        private void OnAddItem(string obj)
        {
            var transaction = new AccountTransactionViewModel(Workspace, null, Model);
            AccountTransactions.Add(transaction);
            SelectedTransaction = transaction;
            RaisePropertyChanged();
        }

        public void OnRowDeleted()
        {
            EventHandler handler = RowDeleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void OnRowInserted()
        {
            EventHandler handler = RowInserted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionDocumentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountTransactionDocument;
        }

        public void DuplicateLastItem()
        {
            var lastTransaction = SelectedTransaction;
            OnAddItem("");
            if (lastTransaction != null)
            {
                var currentTransaction = SelectedTransaction;
                currentTransaction.AccountTransactionType = lastTransaction.AccountTransactionType;
            }
            OnRowInserted();
        }
    }
}
