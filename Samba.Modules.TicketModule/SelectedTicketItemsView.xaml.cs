using System.Windows.Controls;
using System.Windows.Input;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    /// <summary>
    /// Interaction logic for SelectedTicketItemsView.xaml
    /// </summary>
    public partial class SelectedTicketItemsView : UserControl
    {
        public SelectedTicketItemsView()
        {
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
