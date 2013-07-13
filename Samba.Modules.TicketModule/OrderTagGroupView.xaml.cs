using System.Windows.Controls;
using System.Windows.Input;
using Samba.Presentation.Common;

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

        private void DataGrid_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var dg = sender as DataGrid;
            if (dg != null && dg.CurrentColumn is DataGridTemplateColumn)
            {
                if (!dg.IsEditing())
                    dg.BeginEdit();
            }
        }

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var ec = ExtensionServices.GetVisualChild<TextBox>(e.EditingElement as ContentPresenter);
            if (ec != null)
                ec.SelectAll();
        }
    }
}
