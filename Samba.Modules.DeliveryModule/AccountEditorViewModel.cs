using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class AccountEditorViewModel : ObservableObject
    {
        [ImportingConstructor]
        public AccountEditorViewModel()
        {

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
