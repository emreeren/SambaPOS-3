using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule
{
    [Export]
    public class TicketOrdersViewModel : ObservableObject
    {
        [ImportingConstructor]
        public TicketOrdersViewModel()
        {
            //EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(
            //x =>
            //{
            //    if (x.Topic == EventTopicNames.OrderAdded)
            //        RaisePropertyChanged(() => TicketBackground);
            //});
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
