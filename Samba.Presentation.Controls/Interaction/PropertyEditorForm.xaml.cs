using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyTools.Wpf;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Presentation.Controls.Interaction
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.PEHeight = Height;
            Properties.Settings.Default.PEWidth = Width;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_dataGrid != null && _dataGrid.SelectedItem != null)
                InteractionService.UserIntraction.EditProperties(_dataGrid.SelectedItem);
        }

        private void SimpleGrid_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            btnDetails.Visibility = Visibility.Visible;
        }

        private void SimpleGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var selectedItem in (sender as ItemsGrid).SelectedItems)
            {
                InteractionService.UserIntraction.EditProperties(selectedItem);
                e.Handled = true;
            }
        }

        private void MainGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fe = (sender as FrameworkElement);
            if (fe != null)
            {
                var bm = (fe.DataContext as IEditableCollection);
                if (bm != null && bm.EditItemCommand.CanExecute(null))
                    bm.EditItemCommand.Execute(null);
            }
        }

        private void MainGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var fe = (sender as FrameworkElement);
                if (fe != null && ((IEditableCollection)fe.DataContext).EditItemCommand.CanExecute(null))
                    ((IEditableCollection)fe.DataContext).EditItemCommand.Execute(null);
            }
        }
    }
}
