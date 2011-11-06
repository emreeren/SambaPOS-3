using System;
using System.Collections.Generic;
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
using Samba.Presentation.Common;

namespace Samba.Modules.InventoryModule
{
    /// <summary>
    /// Interaction logic for PeriodicConsumptionView.xaml
    /// </summary>
    public partial class PeriodicConsumptionView : UserControl
    {
        public PeriodicConsumptionView()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert)
            {
                //((TransactionViewModel)DataContext).AddTransactionItemCommand.Execute("");
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tc = sender as TabControl;
                if (tc == null) return;
                if (tc.SelectedIndex == 1)
                    ((PeriodicConsumptionViewModel)DataContext).UpdateCost();
            }
        }
    }
}
