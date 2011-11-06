using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;
using Samba.Presentation.Common;

namespace Samba.Modules.DashboardModule
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    /// 

    [Export]
    public partial class DashboardView : UserControl
    {
        [ImportingConstructor]
        public DashboardView(DashboardViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Splitter.Height = new GridLength(0);
            KeyboardPanel.Height = new GridLength(0);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonEventPublisher.PublishDashboardUnloadedEvent(this);
        }
    }
}
