using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;

namespace Samba.Modules.InventoryModule
{
    /// <summary>
    /// Interaction logic for ResourceInventoryView.xaml
    /// </summary>
    
    [Export]
    public partial class WarehouseInventoryView : UserControl
    {
        [ImportingConstructor]
        public WarehouseInventoryView(WarehouseInventoryViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void DataGrid_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
        }

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            
        }
    }
}
