using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services.Common;

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class AccountEditorViewModel : ObservableObject
    {
        [ImportingConstructor]
        public AccountEditorViewModel()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(OnEditAccount);
        }

        private void OnEditAccount(EventParameters<Account> obj)
        {
            if (obj.Topic == EventTopicNames.EditAccountDetails)
            {
                SelectedAccount = new AccountSearchViewModel(obj.Value);
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
    }
}
