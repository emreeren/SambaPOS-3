using System.Windows.Controls;

namespace Samba.Modules.MenuModule
{
    /// <summary>
    /// Interaction logic for TicketTagGroupView.xaml
    /// </summary>
    public partial class TicketTagGroupView : UserControl
    {
        public TicketTagGroupView()
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
