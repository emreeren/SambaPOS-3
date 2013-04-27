using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Samba.Presentation.Common;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for TicketEditorView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class PosView : UserControl
    {
        [ImportingConstructor]
        public PosView(PosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnEvent);
        }

        private void UserControl_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = ((PosViewModel)DataContext).HandleTextInput(e.Text);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.BackgroundFocus();
        }

        private void OnEvent(EventParameters<EventAggregator> obj)
        {
            switch (obj.Topic)
            {
                case EventTopicNames.DisableLandscape:
                    DisableLandscapeMode();
                    break;
                case EventTopicNames.EnableLandscape:
                    EnableLandscapeMode();
                    break;
            }
        }

        private void EnableLandscapeMode()
        {
            LayoutTabControl.SelectedIndex = 0;
            Disconnect(TicketRegion);
            Disconnect(MenuRegion);
            Grid1.Children.Add(TicketRegion);
            Grid1.Children.Add(MenuRegion);
        }

        private void DisableLandscapeMode()
        {
            LayoutTabControl.SelectedIndex = 1;
            Disconnect(TicketRegion);
            Disconnect(MenuRegion);
            Grid2.Items.Add(TicketRegion);
            Grid2.Items.Add(MenuRegion);
            Grid2.SelectedIndex = 0;
        }

        private void Disconnect(FrameworkElement region)
        {
            if (region.Parent is TabControl)
                (region.Parent as TabControl).Items.Remove(region);
            if (region.Parent is Panel)
                (region.Parent as Panel).Children.Remove(region);
        }
    }
}
