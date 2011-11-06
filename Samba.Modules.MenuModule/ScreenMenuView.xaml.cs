using System.Windows.Controls;
using System.Windows.Input;

namespace Samba.Modules.MenuModule
{
    /// <summary>
    /// Interaction logic for ScreenMenuView.xaml
    /// </summary>
    public partial class ScreenMenuView : UserControl
    {
        public ScreenMenuView()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if ((DataContext as ScreenMenuViewModel).SelectedCategory != null)
                (DataContext as ScreenMenuViewModel).EditCategoryItemsCommand.Execute(null);
        }
    }
}
