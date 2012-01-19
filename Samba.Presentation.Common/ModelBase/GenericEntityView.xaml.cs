using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Samba.Presentation.Common.ModelBase
{
    /// <summary>
    /// Interaction logic for GenericEntityView.xaml
    /// </summary>
    public partial class GenericEntityView : UserControl
    {
        public GenericEntityView()
        {
            InitializeComponent();
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
