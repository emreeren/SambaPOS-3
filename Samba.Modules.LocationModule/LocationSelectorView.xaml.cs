using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Samba.Services;

namespace Samba.Modules.LocationModule
{
    /// <summary>
    /// Interaction logic for LocationSelectorView.xaml
    /// </summary>

    [Export]
    public partial class LocationSelectorView : UserControl
    {
        [ImportingConstructor]
        public LocationSelectorView(LocationSelectorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (DiagramCanvas.EditingMode == InkCanvasEditingMode.None)
            {
                if(!AppServices.CurrentLoggedInUser.UserRole.IsAdmin) return;
                brd.BorderBrush = Brushes.Red;
                miDesignMode.IsChecked = true;
                DiagramCanvas.EditingMode = InkCanvasEditingMode.Select;
                (DataContext as LocationSelectorViewModel).LoadTrackableLocations();
            }
            else
            {
                brd.BorderBrush = Brushes.Silver;
                miDesignMode.IsChecked = false;
                DiagramCanvas.EditingMode = InkCanvasEditingMode.None;
                (DataContext as LocationSelectorViewModel).SaveTrackableLocations();
            }
        }
    }
}
