using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Prism.Events;
using Samba.Presentation.ViewModels;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule
{
    /// <summary>
    /// Interaction logic for TicketOrdersView.xaml
    /// </summary>
    [Export]
    public partial class TicketOrdersView : UserControl
    {
        [ImportingConstructor]
        public TicketOrdersView(TicketOrdersViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(
               x =>
               {
                   if (x.Topic == EventTopicNames.OrderAdded)
                       Scroller.ScrollToEnd();
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
