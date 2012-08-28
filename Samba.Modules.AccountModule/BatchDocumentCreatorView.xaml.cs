using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    /// <summary>
    /// Interaction logic for BatchDocumentCreatorView.xaml
    /// </summary>

    [Export]
    public partial class BatchDocumentCreatorView : UserControl
    {
        [ImportingConstructor]
        public BatchDocumentCreatorView(BatchDocumentCreatorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            viewModel.OnUpdate += viewModel_OnUpdate;
        }

        void viewModel_OnUpdate(object sender, EventArgs e)
        {
            var d = DataContext as BatchDocumentCreatorViewModel;
            MainDataGrid.Columns.Where(x => x.MinWidth == 59).ToList().ForEach(x => MainDataGrid.Columns.Remove(x));
            if (d != null)
            {
                var i = 2;
                foreach (var accountTemplate in d.GetNeededAccountTemplates())
                {
                    var accountNameBinding = new Binding(string.Format("[{0}].AccountName", accountTemplate.Id)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                    var itemsSourceBinding = new Binding(string.Format("[{0}].AccountNames", accountTemplate.Id));

                    var dgtc = new DataGridTemplateColumn
                                   {
                                       MinWidth = 59,
                                       Header = accountTemplate.Name,
                                   };

                    var ct = new FrameworkElementFactory(typeof(TextBlock));
                    ct.SetValue(MarginProperty, new Thickness(3));
                    ct.SetBinding(TextBlock.TextProperty, accountNameBinding);
                    dgtc.CellTemplate = new DataTemplate { VisualTree = ct };

                    var cte = new FrameworkElementFactory(typeof(ComboBox));
                    cte.SetValue(ComboBox.IsEditableProperty, true);
                    cte.SetBinding(ItemsControl.ItemsSourceProperty, itemsSourceBinding);
                    cte.SetBinding(ComboBox.TextProperty, accountNameBinding);
                    dgtc.CellEditingTemplate = new DataTemplate { VisualTree = cte };

                    MainDataGrid.Columns.Insert(i, dgtc);
                    i++;
                }
            }

            ICollectionView dataView = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource);
            //clear the existing sort order
            dataView.SortDescriptions.Clear();
            //create a new sort order for the sorting that is done lastly
            dataView.SortDescriptions.Add(new SortDescription("Account.Name", ListSortDirection.Ascending));
            //refresh the view which in turn refresh the grid
            dataView.Refresh();
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
            if (ec != null) ec.SelectAll();

            var ec1 = ExtensionServices.GetVisualChild<ComboBox>(e.EditingElement as ContentPresenter);
            if (ec1 != null) ec1.Focus();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
