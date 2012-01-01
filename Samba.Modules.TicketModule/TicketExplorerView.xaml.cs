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

namespace Samba.Modules.TicketModule
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
