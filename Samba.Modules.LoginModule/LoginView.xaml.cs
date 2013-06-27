using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Prism.Events;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.LoginModule
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>

    [Export]
    public partial class LoginView : UserControl
    {
        [ImportingConstructor]
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnEvent);
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
            Column1.Width = new GridLength(1, GridUnitType.Star);
        }

        private void DisableLandscapeMode()
        {
            Column1.Width = new GridLength(0);
        }

        private void LoginPadControl_PinSubmitted(object sender, string pinValue)
        {
            pinValue.PublishEvent(EventTopicNames.PinSubmitted);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UserControl_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && char.IsDigit(e.Text, 0))
                PadControl.UpdatePinValue(e.Text);
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PadControl.SubmitPin();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Uri u = new Uri(Localization.Properties.Resources.ClientServerConnectionHelpUrlString);
            Process.Start(new ProcessStartInfo(u.AbsoluteUri));
            e.Handled = true;
        }

    }
}
