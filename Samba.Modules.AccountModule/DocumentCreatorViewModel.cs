using System;
using System.ComponentModel.Composition;
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

        [ImportingConstructor]
        public DocumentCreatorViewModel(IAccountService accountService)
        {
            _accountService = accountService;
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
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => Amount);
            RaisePropertyChanged(() => AccountName);
        }

        public string AccountName
        {
            get
            {
                return SelectedAccount == null ? "" : string.Format("{0} {1}: {2}", SelectedAccount.Name, Resources.Balance, _accountService.GetAccountBalance(SelectedAccount).ToString(LocalSettings.DefaultCurrencyFormat));
            }
        }
        public Account SelectedAccount { get; set; }
        public AccountTransactionDocumentTemplate DocumentTemplate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public ICaptionCommand SaveCommand { get; set; }
        public ICaptionCommand CancelCommand { get; set; }

        private void OnCancel(string obj)
        {
            CommonEventPublisher.PublishEntityOperation(SelectedAccount, EventTopicNames.DisplayAccountTransactions);
        }

        private void OnSave(string obj)
        {
            _accountService.CreateNewTransactionDocument(SelectedAccount, DocumentTemplate, Description, Amount);
            CommonEventPublisher.PublishEntityOperation(SelectedAccount, EventTopicNames.DisplayAccountTransactions);
        }

    }
}
