using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule
{
    /// <summary>
    /// Interaction logic for TicketListView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketListView : UserControl
    {
        [ImportingConstructor]
        public TicketListView(TicketListViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.OrderAdded)
                    {
                        Scroller.ScrollToEnd();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.TicketDisplayed)
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
