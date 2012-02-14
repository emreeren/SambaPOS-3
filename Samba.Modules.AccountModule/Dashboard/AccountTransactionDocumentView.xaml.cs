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
