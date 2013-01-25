using System.Windows.Input;
using Samba.Presentation.Common;
using System.Windows.Controls;

namespace Samba.Modules.InventoryModule
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class TransactionView : UserControl
    {
        public TransactionView()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert)
            {
                ((TransactionViewModel)DataContext).AddTransactionItemCommand.Execute("");
                (sender as DataGrid).GetCell(((DataGrid)sender).Items.Count - 1, 0).Focus();
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
