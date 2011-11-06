using System;
using System.Collections.Generic;
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
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Presentation.Terminal
{
    /// <summary>
    /// Interaction logic for MenuItemSelectorView.xaml
    /// </summary>
    public partial class MenuItemSelectorView : UserControl
    {
        public MenuItemSelectorView()
        {
            InitializeComponent();
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketItemViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.TicketItemAdded)
                        Scroller.ScrollToEnd();
                });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Scroller.ScrollToEnd();
        }
    }
}
