using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.MarketModule
{
    [Export]
    public class MarketModuleViewModel : ObservableObject
    {
        private string _activeUrl;
        public string ActiveUrl
        {
            get { return _activeUrl; }
            set { _activeUrl = value; RaisePropertyChanged(() => ActiveUrl); }
        }
    }
}
