using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule.Dashboard
{
    /// <summary>
    /// Interaction logic for AccountTransactionDocumentView.xaml
    /// </summary>
    public partial class AccountTransactionDocumentView : UserControl
    {
        public AccountTransactionDocumentView()
        {
            InitializeComponent();
        }

        internal AccountTransactionDocumentViewModel ViewModel { get { return DataContext as AccountTransactionDocumentViewModel; } }

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

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert || (e.Key == Key.Down && ViewModel.SelectedTransaction == ViewModel.AccountTransactions.LastOrDefault() && ViewModel.SelectedTransaction != null && ViewModel.SelectedTransaction.AccountTransactionType.Id > 0 && ViewModel.SelectedTransaction.Amount > 0))
            {
                ViewModel.DuplicateLastItem();
                e.Handled = true;
            }
        }

        private void DataGrid_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RowInserted += ViewModel_RowInserted;
            ViewModel.RowDeleted += ViewModel_RowDeleted;
        }

        private void DataGrid_OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RowInserted -= ViewModel_RowInserted;
            ViewModel.RowDeleted -= ViewModel_RowDeleted;
        }

        void ViewModel_RowDeleted(object sender, System.EventArgs e)
        {
            DataGrid.BackgroundFocus();
        }

        void ViewModel_RowInserted(object sender, System.EventArgs e)
        {
            var column = 3;
            var row = DataGrid.Items.Count > 0 ? DataGrid.Items.Count - 1 : 0;
            if (ViewModel.SelectedTransaction != null && ViewModel.SelectedTransaction.SourceAccountId == 0) column = 1;
            else if (ViewModel.SelectedTransaction != null && ViewModel.SelectedTransaction.TargetAccountId == 0) column = 2;
            DataGrid.Focus();
            DataGrid.Refresh();
            DataGrid.GetCell(row, column).Focus();
        }
    }
}
