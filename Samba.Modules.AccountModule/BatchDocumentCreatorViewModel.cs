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

        public AccountRowViewModel(Account account, AccountTransactionDocumentTemplate documentTemplate, IAccountService accountService)
        {
            _account = account;
            Amount = accountService.GetDefaultAmount(documentTemplate, account);
            Description = accountService.GetDescription(documentTemplate, account);
        }

        public Account Account
        {
            get { return _account; }
        }

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
    public class BatchDocumentCreatorViewModel
    {
        private readonly IAccountService _accountService;

        public CaptionCommand<string> CreateDocuments { get; set; }

        [ImportingConstructor]
        public BatchDocumentCreatorViewModel(IAccountService accountService)
        {
            Accounts = new ObservableCollection<AccountRowViewModel>();
            _accountService = accountService;
            CreateDocuments = new CaptionCommand<string>("Create Documents", OnCreateDocuments, CanCreateDocument);
        }

        private void OnCreateDocuments(string obj)
        {
            Accounts
                .Where(x => x.IsSelected && x.Amount != 0)
                .AsParallel()
                .SetCulture()
                .ForAll(x => _accountService.CreateNewTransactionDocument(x.Account, SelectedDocumentTemplate, x.Description, x.Amount));
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
                .Select(x => new AccountRowViewModel(x, value, _accountService)));
        }

        public string Description { get; set; }
        public AccountTransactionDocumentTemplate SelectedDocumentTemplate { get; set; }
        public ObservableCollection<AccountRowViewModel> Accounts { get; set; }
    }
}
