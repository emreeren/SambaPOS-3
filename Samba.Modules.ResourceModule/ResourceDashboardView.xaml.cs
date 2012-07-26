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
using Samba.Presentation.Common;

namespace Samba.Modules.ResourceModule
{
    /// <summary>
    /// Interaction logic for ResourceDashboardView.xaml
    /// </summary>
   
    [Export]
    public partial class ResourceDashboardView : UserControl
    {
        [ImportingConstructor]
        public ResourceDashboardView(ResourceDashboardViewModel viewModel)
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
                ((ResourceDashboardViewModel)DataContext).LoadTrackableResourceScreenItems();
            }
            else
            {
                brd.BorderBrush = Brushes.Silver;
                miDesignMode.IsChecked = false;
                DiagramCanvas.EditingMode = InkCanvasEditingMode.None;
                ((ResourceDashboardViewModel)DataContext).SaveTrackableResourceScreenItems();
            }
        }

        private void miAddWidget_Click(object sender, RoutedEventArgs e)
        {
            ((ResourceDashboardViewModel)DataContext).AddWidget();
        }

        private void DiagramCanvas_WidgetRemoved(object sender, EventArgs e)
        {
            ((ResourceDashboardViewModel)DataContext).RemoveWidget(sender as WidgetViewModel);
        }
    }
}
