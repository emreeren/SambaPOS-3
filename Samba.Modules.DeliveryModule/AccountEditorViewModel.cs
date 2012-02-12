using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class AccountEditorViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountEditorViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(OnEditAccount);
        }

        private void OnEditAccount(EventParameters<Account> obj)
        {
            if (obj.Topic == EventTopicNames.EditAccountDetails)
            {
                var accountTemplate = _cacheService.GetAccountTemplateById(obj.Value.AccountTemplateId);
                SelectedAccount = new AccountSearchViewModel(obj.Value);
                CustomDataViewModel = new AccountCustomDataViewModel(obj.Value, accountTemplate);
                RaisePropertyChanged(() => CustomDataViewModel);
            }
        }

        private AccountSearchViewModel _selectedAccount;
        public AccountSearchViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        public AccountCustomDataViewModel CustomDataViewModel { get; set; }
    }
}
