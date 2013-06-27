using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Samba.Infrastructure.Settings;
using Samba.Services;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for MessageClientStatusView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class MessageClientStatusView : UserControl
    {
        private readonly IMessagingService _messagingService;
        private readonly Timer _timer;

        private void OnTimerTick(object state)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new Action(delegate
                {
                    if (_messagingService.IsConnected)
                    {
                        StatusLabel.Content = Properties.Resources.Connected;
                        StatusLabel.Foreground = Brushes.Green;
                        _timer.Change(60000, 60000);
                    }
                    else
                    {
                        StatusLabel.Content = Properties.Resources.MessageServerError;
                        StatusLabel.Foreground = Brushes.Red;
                        _timer.Change(10000, 10000);
                    }
                }));
        }

        [ImportingConstructor]
        public MessageClientStatusView(IMessagingService messagingService)
        {
            _messagingService = messagingService;
            InitializeComponent();
            Application.Current.MainWindow.Closing += MainWindow_Closing;

            _timer = new Timer(OnTimerTick, null, Timeout.Infinite, 1000);
            if (LocalSettings.StartMessagingClient)
            {
                _timer.Change(10000, 10000);
            }
            else StatusLabel.Visibility = System.Windows.Visibility.Collapsed;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Dispose();
        }
    }
}
