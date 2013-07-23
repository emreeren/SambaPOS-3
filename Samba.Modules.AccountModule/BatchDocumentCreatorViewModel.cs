using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class BatchDocumentCreatorViewModel : ObservableObject
    {
        public event EventHandler OnUpdate;
        private void OnOnUpdate(EventArgs e)
        {
            EventHandler handler = OnUpdate;
            if (handler != null) handler(this, e);
        }

        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;
        private readonly IPrinterService _printerService;
        private readonly IApplicationState _applicationState;
        private AccountTransactionDocumentType _selectedDocumentType;

        public CaptionCommand<string> CreateDocuments { get; set; }
        public CaptionCommand<string> GoBack { get; set; }
        public CaptionCommand<string> Print { get; set; }

        [ImportingConstructor]
        public BatchDocumentCreatorViewModel(IAccountService accountService, ICacheService cacheService,
            IPrinterService printerService, IApplicationState applicationState)
        {
            Accounts = new ObservableCollection<AccountRowViewModel>();
            _accountService = accountService;
            _cacheService = cacheService;
            _printerService = printerService;
            _applicationState = applicationState;
            CreateDocuments = new CaptionCommand<string>(string.Format(Resources.Create_f, "").Trim(), OnCreateDocuments, CanCreateDocument);
            GoBack = new CaptionCommand<string>(Resources.Back, OnGoBack);
            Print = new CaptionCommand<string>(Resources.Print, OnPrint);
        }

        public string Title { get { return SelectedDocumentType != null ? SelectedDocumentType.Name : ""; } }
        public string Description { get; set; }
        public ObservableCollection<AccountRowViewModel> Accounts { get; set; }
        public bool IsPrintButtonVisible { get { return SelectedDocumentType != null && SelectedPrinterTemplate != null && SelectedPrinter != null; } }

        public AccountTransactionDocumentType SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                if (Equals(value, _selectedDocumentType)) return;
                _selectedDocumentType = value;
                RaisePropertyChanged(() => SelectedDocumentType);
                RaisePropertyChanged(() => Title);
                RaisePropertyChanged(() => IsPrintButtonVisible);
            }
        }

        public Printer SelectedPrinter { get { return _applicationState.GetTransactionPrinter(); } }
        public PrinterTemplate SelectedPrinterTemplate
        {
            get
            {
                try
                {
                    return _cacheService.GetPrinterTemplates().FirstOrDefault(x => x.Id == SelectedDocumentType.PrinterTemplateId);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            }
        }

        private void OnGoBack(string obj)
        {
            SelectedDocumentType.PublishEvent(EventTopicNames.BatchDocumentsCreated);
        }

        public IEnumerable<AccountType> GetNeededAccountTypes()
        {
            return SelectedDocumentType.GetNeededAccountTypes().Select(x => _cacheService.GetAccountTypeById(x));
        }

        private void OnPrint(string obj)
        {
            BatchCreate(CreatePrintDocument);
        }

        private void OnCreateDocuments(string obj)
        {
            BatchCreate(CreateDocument);
        }

        private void BatchCreate(Func<AccountRowViewModel, AccountTransactionDocument> action)
        {
            if (Accounts.Where(x => x.IsSelected).Any(x => x.TargetAccounts.Any(y => y.SelectedAccountId == 0))) return;
            Accounts
                .Where(x => x.IsSelected && x.Amount != 0)
                .ToList()
                .ForEach(x => action(x));

            SelectedDocumentType.PublishEvent(EventTopicNames.BatchDocumentsCreated);
        }

        private AccountTransactionDocument CreateDocument(AccountRowViewModel accountRowViewModel)
        {
            var document = _accountService.CreateTransactionDocument(accountRowViewModel.Account,
                                                       SelectedDocumentType, accountRowViewModel.Description,
                                                       accountRowViewModel.Amount,
                                                       accountRowViewModel.TargetAccounts.Select(
                                                           y =>
                                                           new Account
                                                           {
                                                               Id = y.SelectedAccountId,
                                                               AccountTypeId = y.AccountType.Id
                                                           }));
            _applicationState.NotifyEvent(RuleEventNames.AccountTransactionDocumentCreated, new
              {
                  AccountTransactionDocumentName = SelectedDocumentType.Name,
                  DocumentId = document.Id
              });

            return document;
        }

        private AccountTransactionDocument CreatePrintDocument(AccountRowViewModel accountRowViewModel)
        {
            if (SelectedPrinterTemplate == null) return null;
            if (SelectedPrinter == null) return null;
            var document = CreateDocument(accountRowViewModel);
            _printerService.PrintObject(document, SelectedPrinter, SelectedPrinterTemplate);
            return document;
        }

        private bool CanCreateDocument(string arg)
        {
            return Accounts.Any(x => x.IsSelected);
        }

        public void Update(AccountTransactionDocumentType value)
        {
            SelectedDocumentType = value;
            Accounts.Clear();
            var accounts = GetAccounts(value);
            Accounts.AddRange(accounts
                .AsParallel()
                .SetCulture()
                .Select(x => new AccountRowViewModel(x, value, _accountService, _cacheService)));
            OnOnUpdate(EventArgs.Empty);
        }

        private IEnumerable<Account> GetAccounts(AccountTransactionDocumentType documentType)
        {
            return _accountService.GetDocumentAccounts(documentType).Where(documentType.CanMakeAccountTransaction);
        }

    }
}
