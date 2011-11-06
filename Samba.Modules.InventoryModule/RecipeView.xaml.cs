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
    /// Interaction logic for RecipeView.xaml
    /// </summary>
    public partial class RecipeView : UserControl
    {
        public RecipeView()
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

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert)
            {
                ((RecipeViewModel)DataContext).AddInventoryItemCommand.Execute("");
                (sender as DataGrid).GetCell(((DataGrid)sender).Items.Count - 1, 0).Focus();
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
