using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.DashboardModule
{
    /// <summary>
    /// Interaction logic for KeyboardButtonView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class KeyboardButtonView : UserControl
    {
        private readonly DashboardView _dashboardView;
        private readonly GridLength _gridLength = new GridLength(5);
        private readonly GridLength _zeroGridLength = new GridLength(0);

        [ImportingConstructor]
        public KeyboardButtonView(DashboardView dashboardView)
        {
            InitializeComponent();
            _dashboardView = dashboardView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToggleKeyboard();
        }

        private void ToggleKeyboard()
        {
            if (AppServices.ActiveAppScreen == AppScreens.Dashboard)
            {
                if (_dashboardView.Splitter.Height == _zeroGridLength)
                {
                    _dashboardView.Splitter.Height = _gridLength;
                    _dashboardView.KeyboardPanel.Height = GridLength.Auto;
                }
                else
                {
                    _dashboardView.Splitter.Height = _zeroGridLength;
                    _dashboardView.KeyboardPanel.Height = _zeroGridLength;
                }
            }
            else
            {
                InteractionService.ToggleKeyboard();
            }
        }
    }
}
