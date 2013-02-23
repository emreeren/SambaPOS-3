using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Samba.Modules.TicketModule
{
    [Export]
    public partial class TicketExplorerView : UserControl
    {
        private bool _scrolled;

        public TicketExplorerView()
        {
            InitializeComponent();
        }

        private void DisplayTicket()
        {
            var tex = DataContext as TicketExplorerViewModel;
            if (tex != null)
            {
                tex.QueueDisplayTicket();
            }
        }

        private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_scrolled)
            {
                DisplayTicket();
            }
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _scrolled = false;
        }

        private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _scrolled = e.VerticalChange > 0 || e.HorizontalChange > 0;
        }

        private void DataGrid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                DisplayTicket();
            }
        }

        private void DataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var i = 0;
            var d = DataContext as TicketExplorerViewModel;
            if (d == null) return;

            var entityTypes = d.EntityTypes.Select(entityType => new DataGridTextColumn
                {
                    Header = entityType.EntityName,
                    Binding = new Binding("[" + entityType.Id + "]"),
                    MinWidth = 60,
                });

            foreach (var dgtc in entityTypes)
            {
                DataGrid.Columns.Insert(i + 1, dgtc);
                i++;
            }
        }
    }
}
