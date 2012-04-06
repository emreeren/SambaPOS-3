using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Samba.Modules.ResourceModule
{
    /// <summary>
    /// Interaction logic for LocationSelectorView.xaml
    /// </summary>

    [Export]
    public partial class ResourceSelectorView : UserControl
    {
        [ImportingConstructor]
        public ResourceSelectorView(ResourceSelectorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (DiagramCanvas.EditingMode == InkCanvasEditingMode.None)
            {
                brd.BorderBrush = Brushes.Red;
                miDesignMode.IsChecked = true;
                DiagramCanvas.EditingMode = InkCanvasEditingMode.Select;
                ((ResourceSelectorViewModel)DataContext).LoadTrackableResourceScreenItems();
            }
            else
            {
                brd.BorderBrush = Brushes.Silver;
                miDesignMode.IsChecked = false;
                DiagramCanvas.EditingMode = InkCanvasEditingMode.None;
                ((ResourceSelectorViewModel)DataContext).SaveTrackableResourceScreenItems();
            }
        }
    }
}
