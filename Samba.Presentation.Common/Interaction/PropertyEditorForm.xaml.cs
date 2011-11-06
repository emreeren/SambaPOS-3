using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyTools.Wpf;
using Samba.Presentation.Common.Services;

namespace Samba.Presentation.Common.Interaction
{
    /// <summary>
    /// Interaction logic for PropertyEditorForm.xaml
    /// </summary>
    public partial class PropertyEditorForm : Window
    {
        private DataGrid _dataGrid;
        public PropertyEditorForm()
        {
            InitializeComponent();
            Height = Properties.Settings.Default.PEHeight;
            Width = Properties.Settings.Default.PEWidth;
            btnDetails.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PropertyGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (sender is DataGrid)
            {
                _dataGrid = sender as DataGrid;
                btnDetails.Visibility = Visibility.Visible;
            }

            var browsable = ((PropertyDescriptor)e.PropertyDescriptor).Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
            if (browsable != null && !browsable.Browsable)
            {
                e.Cancel = true;
                return;
            }

            var displayName = ((PropertyDescriptor)e.PropertyDescriptor).Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
            if (displayName != null)
            {
                e.Column.Header = displayName.DisplayName;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.PEHeight = Height;
            Properties.Settings.Default.PEWidth = Width;
        }

        private void PropertyGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                InteractionService.UserIntraction.EditProperties(((DataGrid)sender).SelectedItem);
            }
        }

        private void PropertyGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.RightCtrl))
            {
                InteractionService.UserIntraction.EditProperties(((DataGrid)sender).SelectedItem);
                e.Handled = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_dataGrid != null && _dataGrid.SelectedItem != null)
                InteractionService.UserIntraction.EditProperties(_dataGrid.SelectedItem);
        }

        private void PropertyGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox)
            {
                ((TextBox)e.EditingElement).Text = ((TextBox)e.EditingElement).Text.Replace("\b", "");
            }
        }

        private void SimpleGrid_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            btnDetails.Visibility = Visibility.Visible;
        }

        private void SimpleGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var selectedItem in (sender as SimpleGrid).SelectedItems)
            {
                InteractionService.UserIntraction.EditProperties(selectedItem);
                e.Handled = true;
            }
        }
    }
}
