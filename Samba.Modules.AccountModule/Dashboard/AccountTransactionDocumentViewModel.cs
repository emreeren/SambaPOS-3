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

        [ImportingConstructor]
        public AccountTransactionDocumentViewModel(ICacheService cacheService, IPrinterService printerService, IApplicationState applicationState)
        {
            _cacheService = cacheService;
            _printerService = printerService;
            _applicationState = applicationState;
            AddItemCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Line), OnAddItem);
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
        public ICaptionCommand PrintCommand { get; set; }

        private ObservableCollection<AccountTransactionViewModel> _accountTransactions;
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

        private void OnAddItem(string obj)
        {
            var transaction = new AccountTransactionViewModel(Workspace, null, Model);
            AccountTransactions.Add(transaction);
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionDocumentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountTransactionDocument;
        }
    }
}
