using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Samba.Modules.PosModule.WidgetCreators
{
    /// <summary>
    /// Interaction logic for TicketExplorerView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketExplorerView : UserControl
    {
        private bool _scrolled;

        public TicketExplorerView()
        {
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

        private void DataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var i = 0;
            var d = DataContext as TicketExplorerViewModel;
            if (d != null)
            {
                foreach (var resourceTemplate in d.ResourceTemplates)
                {
                    DataGridColumn dgtc = new DataGridTextColumn
                                              {
                        Header = resourceTemplate.EntityName,
                        Binding = new Binding("[" + resourceTemplate.Id + "]"),
                        MinWidth = 60,
                    };
                    DataGrid.Columns.Insert(i + 1, dgtc);
                    i++;
                }
            }
        }
    }
}
