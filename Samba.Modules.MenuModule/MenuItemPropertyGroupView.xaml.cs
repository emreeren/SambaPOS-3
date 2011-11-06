using System.Windows.Controls;

namespace Samba.Modules.MenuModule
{
    /// <summary>
    /// Interaction logic for MenuItemPropertyGroupView.xaml
    /// </summary>
    public partial class MenuItemPropertyGroupView : UserControl
    {
        public MenuItemPropertyGroupView()
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
