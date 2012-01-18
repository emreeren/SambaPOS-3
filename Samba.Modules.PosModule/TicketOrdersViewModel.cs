using System.ComponentModel.Composition;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketOrdersViewModel : ObservableObject
    {
        [ImportingConstructor]
        public TicketOrdersViewModel()
        {

        }

        public string TicketBackground { get { return SelectedTicket != null && (SelectedTicket.IsLocked || SelectedTicket.IsPaid) ? "Transparent" : "White"; } }

        private TicketViewModel _selectedTicket;
        public TicketViewModel SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                RaisePropertyChanged(() => SelectedTicket);
                RaisePropertyChanged(() => TicketBackground);
            }
        }
    }
}
