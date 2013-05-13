using System.Linq;
using System.Windows.Input;
using Samba.Presentation.Common;
using System.Windows.Controls;

namespace Samba.Modules.InventoryModule
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class TransactionDocumentView : UserControl
    {
        public TransactionDocumentView()
        {
            InitializeComponent();
        }

        public TransactionDocumentViewModel ViewModel { get { return DataContext as TransactionDocumentViewModel; } }


        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert || (e.Key == Key.Down && ViewModel.SelectedTransactionItem == ViewModel.TransactionItems.LastOrDefault()))
            {
                ViewModel.ExecuteTransactionItemCommand();
                e.Handled = true;
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

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RowInserted += ViewModel_RowInserted;
            ViewModel.RowDeleted += ViewModel_RowDeleted;
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RowInserted -= ViewModel_RowInserted;
            ViewModel.RowDeleted -= ViewModel_RowDeleted;
        }

        void ViewModel_RowDeleted(object sender, System.EventArgs e)
        {
            InventoryTransactionGrid.BackgroundFocus();
        }

        void ViewModel_RowInserted(object sender, System.EventArgs e)
        {
            var column = 0;
            var row = InventoryTransactionGrid.Items.Count > 0 ? InventoryTransactionGrid.Items.Count - 1 : 0;
            if (ViewModel.SelectedTransactionItem.InventoryTransactionType != null)
                column = InventoryTransactionGrid.Columns.Single(x => x.Header.ToString() == Localization.Properties.Resources.InventoryItemName).DisplayIndex;
            InventoryTransactionGrid.Focus();
            InventoryTransactionGrid.Refresh();
            InventoryTransactionGrid.GetCell(row, column).Focus();
        }
    }
}
