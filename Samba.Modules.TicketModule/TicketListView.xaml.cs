using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.TicketModule
{
    /// <summary>
    /// Interaction logic for TicketListView.xaml
    /// </summary>
    public partial class TicketListView : UserControl
    {
        public TicketListView()
        {
            InitializeComponent();
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketItemViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.TicketItemAdded)
                        Scroller.ScrollToEnd();
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectedTicketChanged)
                        Scroller.ScrollToEnd();
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.DisplayTicketView || x.Topic == EventTopicNames.RefreshSelectedTicket)
                        Scroller.ScrollToEnd();
                });
        }
    }
}
