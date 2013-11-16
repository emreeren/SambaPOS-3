using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;
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
    public class DocumentCreatorViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;
        private readonly IPrinterService _printerService;
        private readonly IApplicationState _applicationState;
        private string _description;

        [ImportingConstructor]
        public DocumentCreatorViewModel(IAccountService accountService, ICacheService cacheService, IPrinterService printerService, IApplicationState applicationState)
        {
            _accountService = accountService;
            _cacheService = cacheService;
            _printerService = printerService;
            _applicationState = applicationState;
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave);
            PrintCommand = new CaptionCommand<string>("Print", OnPrint);
            CancelCommand = new CaptionCommand<string>(Resources.Cancel, OnCancel);
            EventServiceFactory.EventService.GetEvent<GenericEvent<DocumentCreationData>>().Subscribe(OnDocumentCreation);
        }

        private void OnDocumentCreation(EventParameters<DocumentCreationData> obj)
        {
            _description = _accountService.GetDescription(obj.Value.DocumentType, obj.Value.Account);
            SelectedAccount = obj.Value.Account;
            DocumentType = obj.Value.DocumentType;
            Description = _description;
            Amount = _accountService.GetDefaultAmount(obj.Value.DocumentType, obj.Value.Account);
            AccountSelectors = GetAccountSelectors().ToList();

            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => Amount);
            RaisePropertyChanged(() => AccountName);
            RaisePropertyChanged(() => IsPrintCommandVisible);
        }

        private IEnumerable<AccountSelectViewModel> GetAccountSelectors()
        {
            var map = DocumentType.AccountTransactionDocumentAccountMaps.FirstOrDefault(x => x.AccountId == SelectedAccount.Id);
            return map != null
                ? DocumentType.GetNeededAccountTypes().Select(x => new AccountSelectViewModel(_accountService, _cacheService.GetAccountTypeById(x), map.MappedAccountId, map.MappedAccountName))
                : DocumentType.GetNeededAccountTypes().Select(x => new AccountSelectViewModel(_accountService, _cacheService.GetAccountTypeById(x)));
        }

        public string AccountName
        {
            get
            {
                return SelectedAccount == null ? "" : string.Format("{0} {1}: {2}", SelectedAccount.Name, Resources.Balance, GetAccountBalance());
            }
        }

        private string GetAccountBalance()
        {
            if (SelectedAccount.ForeignCurrencyId > 0)
                return
                    _accountService.GetAccountExchangeBalance(SelectedAccount.Id)
                        .ToString(LocalSettings.ReportCurrencyFormat);
            return _accountService.GetAccountBalance(SelectedAccount.Id).ToString(LocalSettings.ReportCurrencyFormat);
        }

        public Account SelectedAccount { get; set; }
        public AccountTransactionDocumentType DocumentType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPrintCommandVisible { get { return DocumentType != null && DocumentType.PrinterTemplateId > 0; } }
        public ICaptionCommand SaveCommand { get; set; }
        public ICaptionCommand PrintCommand { get; set; }
        public ICaptionCommand CancelCommand { get; set; }
        public Printer SelectedPrinter { get { return _applicationState.GetTransactionPrinter(); } }
        public PrinterTemplate SelectedPrinterTemplate { get { return _cacheService.GetPrinterTemplates().First(x => x.Id == DocumentType.PrinterTemplateId); } }

        private IEnumerable<AccountSelectViewModel> _accountSelectors;
        public IEnumerable<AccountSelectViewModel> AccountSelectors
        {
            get { return _accountSelectors; }
            set
            {
                _accountSelectors = value;
                RaisePropertyChanged(() => AccountSelectors);
            }
        }

        private void OnCancel(string obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData(SelectedAccount), EventTopicNames.DisplayAccountTransactions);
        }

        private void OnSave(string obj)
        {
            Action(CreateDocument);
        }

        private void OnPrint(string obj)
        {
            Action(PrintDocument);
        }

        public void Action(Func<AccountTransactionDocument> action)
        {
            var document = action();
            if (document != null)
            {
                _applicationState.NotifyEvent(RuleEventNames.AccountTransactionDocumentCreated, new { AccountTransactionDocumentName = DocumentType.Name, DocumentId = document.Id });
                CommonEventPublisher.PublishEntityOperation(new AccountData(SelectedAccount), EventTopicNames.DisplayAccountTransactions);
            }
        }

        public AccountTransactionDocument CreateDocument()
        {
            var description = Description;
            if (Description != _description) description = string.Format("{0}  [{1}]", _description, Description);
            if (AccountSelectors.Any(x => x.SelectedAccountId == 0)) return null;
            return _accountService.CreateTransactionDocument(SelectedAccount, DocumentType, description, Amount, AccountSelectors.Select(x => new Account { Id = x.SelectedAccountId, AccountTypeId = x.AccountType.Id }));
        }

        public AccountTransactionDocument PrintDocument()
        {
            if (SelectedPrinter == null) return null;
            if (SelectedPrinterTemplate == null) return null;
            var document = CreateDocument();
            if (document == null) return null;
            _printerService.PrintObject(document, SelectedPrinter, SelectedPrinterTemplate);
            return document;
        }
    }
}
