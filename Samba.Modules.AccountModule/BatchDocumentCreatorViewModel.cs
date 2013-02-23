using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

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
        private AccountTransactionDocumentType _selectedDocumentType;

        public CaptionCommand<string> CreateDocuments { get; set; }

        [ImportingConstructor]
        public BatchDocumentCreatorViewModel(IAccountService accountService, ICacheService cacheService)
        {
            Accounts = new ObservableCollection<AccountRowViewModel>();
            _accountService = accountService;
            _cacheService = cacheService;
            CreateDocuments = new CaptionCommand<string>(string.Format(Resources.Create_f, "").Trim(), OnCreateDocuments, CanCreateDocument);
        }

        public IEnumerable<AccountType> GetNeededAccountTypes()
        {
            return
                SelectedDocumentType.GetNeededAccountTypes().Select(x => _cacheService.GetAccountTypeById(x));
        }

        private void OnCreateDocuments(string obj)
        {
            if (Accounts.Where(x => x.IsSelected).Any(x => x.TargetAccounts.Any(y => y.SelectedAccountId == 0))) return;
            Accounts
                .Where(x => x.IsSelected && x.Amount != 0)
                .AsParallel()
                .SetCulture()
                .ForAll(x => _accountService.CreateTransactionDocument(x.Account, SelectedDocumentType, x.Description, x.Amount, x.TargetAccounts.Select(y => new Account { Id = y.SelectedAccountId, AccountTypeId = y.AccountType.Id })));
            SelectedDocumentType.PublishEvent(EventTopicNames.BatchDocumentsCreated);
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
            return _accountService.GetDocumentAccounts(documentType);
        }

        public string Title { get { return SelectedDocumentType != null ? SelectedDocumentType.Name : ""; } }
        public string Description { get; set; }
        public ObservableCollection<AccountRowViewModel> Accounts { get; set; }

        public AccountTransactionDocumentType SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                if (Equals(value, _selectedDocumentType)) return;
                _selectedDocumentType = value;
                RaisePropertyChanged(() => SelectedDocumentType);
                RaisePropertyChanged(() => Title);
            }
        }

    }
}
