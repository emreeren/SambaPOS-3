using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
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
        private AccountTransactionDocumentTemplate _selectedDocumentTemplate;

        public CaptionCommand<string> CreateDocuments { get; set; }

        [ImportingConstructor]
        public BatchDocumentCreatorViewModel(IAccountService accountService, ICacheService cacheService)
        {
            Accounts = new ObservableCollection<AccountRowViewModel>();
            _accountService = accountService;
            _cacheService = cacheService;
            CreateDocuments = new CaptionCommand<string>(string.Format(Resources.Create_f, "").Trim(), OnCreateDocuments, CanCreateDocument);
        }

        public IEnumerable<AccountTemplate> GetNeededAccountTemplates()
        {
            return
                SelectedDocumentTemplate.GetNeededAccountTemplates().Select(x => _cacheService.GetAccountTemplateById(x));
        }

        private void OnCreateDocuments(string obj)
        {
            if (Accounts.Where(x => x.IsSelected).Any(x => x.TargetAccounts.Any(y => y.SelectedAccountId == 0))) return;
            Accounts
                .Where(x => x.IsSelected && x.Amount != 0)
                .AsParallel()
                .SetCulture()
                .ForAll(x => _accountService.CreateNewTransactionDocument(x.Account, SelectedDocumentTemplate, x.Description, x.Amount, x.TargetAccounts.Select(y => new Account { Id = y.SelectedAccountId, AccountTemplateId = y.AccountTemplate.Id })));
            SelectedDocumentTemplate.PublishEvent(EventTopicNames.BatchDocumentsCreated);
        }

        private bool CanCreateDocument(string arg)
        {
            return Accounts.Any(x => x.IsSelected);
        }

        public void Update(AccountTransactionDocumentTemplate value)
        {
            SelectedDocumentTemplate = value;
            Accounts.Clear();
            var accounts = GetAccounts(value);
            Accounts.AddRange(accounts
                .AsParallel()
                .SetCulture()
                .Select(x => new AccountRowViewModel(x, value, _accountService, _cacheService)));
            OnOnUpdate(EventArgs.Empty);
        }

        private IEnumerable<Account> GetAccounts(AccountTransactionDocumentTemplate documentTemplate)
        {
            return _accountService.GetDocumentAccounts(documentTemplate);
        }

        public string Title { get { return SelectedDocumentTemplate != null ? SelectedDocumentTemplate.Name : ""; } }
        public string Description { get; set; }
        public ObservableCollection<AccountRowViewModel> Accounts { get; set; }

        public AccountTransactionDocumentTemplate SelectedDocumentTemplate
        {
            get { return _selectedDocumentTemplate; }
            set
            {
                if (Equals(value, _selectedDocumentTemplate)) return;
                _selectedDocumentTemplate = value;
                RaisePropertyChanged(() => SelectedDocumentTemplate);
                RaisePropertyChanged(() => Title);
            }
        }

    }
}
