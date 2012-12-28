using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samba.Modules.InventoryModule
{
    /// <summary>
    /// Interaction logic for ResourceInventoryView.xaml
    /// </summary>
    
    [Export]
    public partial class ResourceInventoryView : UserControl
    {
        [ImportingConstructor]
        public ResourceInventoryView(ResourceInventoryViewModel viewModel)
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
