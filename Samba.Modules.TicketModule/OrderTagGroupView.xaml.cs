using System.Windows.Controls;

namespace Samba.Modules.TicketModule
{
    /// <summary>
    /// Interaction logic for MenuItemPropertyGroupView.xaml
    /// </summary>
    public partial class OrderTagGroupView : UserControl
    {
        public OrderTagGroupView()
        {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox)
            {
                ((TextBox)e.EditingElement).Text = ((TextBox)e.EditingElement).Text.Replace("\b", "");
            }
        }
    }
}
