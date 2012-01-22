using System.ComponentModel.Composition;
using System.Windows.Controls;
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    /// <summary>
    /// Interaction logic for SelectedOrdersView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class SelectedOrdersView : UserControl
    {
        [ImportingConstructor]
        public SelectedOrdersView(SelectedOrdersViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void GroupBox_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (((Control)sender).IsVisible)
                ExtraPropertyName.BackgroundFocus();
        }

        private void GroupBox_IsVisibleChanged_1(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (((Control)sender).IsVisible)
                TicketNote.BackgroundFocus();
        }

        private void GroupBox_IsVisibleChanged_2(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (((Control)sender).IsVisible)
                FreeTag.BackgroundFocus();
        }
    }
}
