using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for TicketExplorerView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketExplorerView : UserControl
    {
        private bool _scrolled;

        [ImportingConstructor]
        public TicketExplorerView(TicketExplorerViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_scrolled)
                (DataContext as TicketExplorerViewModel).QueueDisplayTicket();
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _scrolled = false;
        }

        private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _scrolled = true;
        }

        private void DataGrid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
                (DataContext as TicketExplorerViewModel).QueueDisplayTicket();
        }
    }
}
