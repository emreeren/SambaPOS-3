using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class DocumentCreatorViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public DocumentCreatorViewModel(IAccountService accountService, ICacheService cacheService)
        {
            _accountService = accountService;
            _cacheService = cacheService;
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave);
            CancelCommand = new CaptionCommand<string>(Resources.Cancel, OnCancel);
            EventServiceFactory.EventService.GetEvent<GenericEvent<DocumentCreationData>>().Subscribe(OnDocumentCreation);
        }

        private void OnDocumentCreation(EventParameters<DocumentCreationData> obj)
        {
            SelectedAccount = obj.Value.Account;
            DocumentTemplate = obj.Value.DocumentTemplate;
            Description = _accountService.GetDescription(obj.Value.DocumentTemplate, obj.Value.Account);
            Amount = _accountService.GetDefaultAmount(obj.Value.DocumentTemplate, obj.Value.Account);
            AccountSelectors = DocumentTemplate.GetNeededAccountTemplates().Select(x => new AccountSelectViewModel(_accountService, _cacheService.GetAccountTemplateById(x))).ToList();
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => Amount);
            RaisePropertyChanged(() => AccountName);
        }

        public string AccountName
        {
            get
            {
                return SelectedAccount == null ? "" : string.Format("{0} {1}: {2}", SelectedAccount.Name, Resources.Balance, _accountService.GetAccountBalance(SelectedAccount.Id).ToString(LocalSettings.DefaultCurrencyFormat));
            }
        }

        public Account SelectedAccount { get; set; }
        public AccountTransactionDocumentTemplate DocumentTemplate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public ICaptionCommand SaveCommand { get; set; }
        public ICaptionCommand CancelCommand { get; set; }

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
            CommonEventPublisher.PublishEntityOperation(new AccountData { AccountId = SelectedAccount.Id }, EventTopicNames.DisplayAccountTransactions);
        }

        private void OnSave(string obj)
        {
            if (AccountSelectors.Any(x => x.SelectedAccountId == 0)) return;
            _accountService.CreateNewTransactionDocument(SelectedAccount, DocumentTemplate, Description, Amount, AccountSelectors.Select(x => new Account { Id = x.SelectedAccountId, AccountTemplateId = x.AccountTemplate.Id }));
            CommonEventPublisher.PublishEntityOperation(new AccountData { AccountId = SelectedAccount.Id }, EventTopicNames.DisplayAccountTransactions);
        }
    }
}
