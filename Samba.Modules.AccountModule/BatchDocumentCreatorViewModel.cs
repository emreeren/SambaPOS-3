using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    public class AccountRowViewModel : ObservableObject
    {
        private readonly Account _account;

        public AccountRowViewModel(Account account, AccountTransactionDocumentTemplate documentTemplate, IAccountService accountService, ICacheService cacheService)
        {
            _account = account;
            Amount = accountService.GetDefaultAmount(documentTemplate, account);
            Description = accountService.GetDescription(documentTemplate, account);
            TargetAccounts = documentTemplate.GetNeededAccountTemplates().Select(x => new AccountSelectViewModel(accountService, cacheService.GetAccountTemplateById(x))).ToList();
        }

        public AccountSelectViewModel this[int accountTemplateId] { get { return TargetAccounts.Single(x => x.AccountTemplate.Id == accountTemplateId); } }

        public Account Account
        {
            get { return _account; }
        }

        public IList<AccountSelectViewModel> TargetAccounts { get; set; }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged(() => Description);
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                IsSelected = _amount != 0;
                RaisePropertyChanged(() => Amount);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }
    }

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

        public CaptionCommand<string> CreateDocuments { get; set; }

        [ImportingConstructor]
        public BatchDocumentCreatorViewModel(IAccountService accountService, ICacheService cacheService)
        {
            Accounts = new ObservableCollection<AccountRowViewModel>();
            _accountService = accountService;
            _cacheService = cacheService;
            CreateDocuments = new CaptionCommand<string>("Create Documents", OnCreateDocuments, CanCreateDocument);
        }

        public IEnumerable<AccountTemplate> GetNeededAccountTemplates()
        {
            return
                SelectedDocumentTemplate.GetNeededAccountTemplates().Select(x => _cacheService.GetAccountTemplateById(x));
        }

        private void OnCreateDocuments(string obj)
        {
            if (Accounts.Where(x=>x.IsSelected).Any(x => x.TargetAccounts.Any(y => y.SelectedAccountId == 0))) return;
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
            var accounts = SelectedDocumentTemplate.Filter == 1
                ? _accountService.GetBalancedAccounts(value.MasterAccountTemplateId)
                : (_accountService.GetAccounts(value.MasterAccountTemplateId));
            Accounts.AddRange(accounts
                .AsParallel()
                .SetCulture()
                .Select(x => new AccountRowViewModel(x, value, _accountService, _cacheService)));
            OnOnUpdate(EventArgs.Empty);
        }

        public string Description { get; set; }
        public AccountTransactionDocumentTemplate SelectedDocumentTemplate { get; set; }
        public ObservableCollection<AccountRowViewModel> Accounts { get; set; }
    }
}
