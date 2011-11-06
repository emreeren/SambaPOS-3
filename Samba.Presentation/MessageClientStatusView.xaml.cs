using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Samba.Infrastructure.Settings;
using Samba.Services;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for MessageClientStatusView.xaml
    /// </summary>
    public partial class MessageClientStatusView : UserControl
    {
        private readonly Timer _timer;

        private void OnTimerTick(object state)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new Action(delegate
                {
                    if (AppServices.MessagingService.IsConnected)
                    {
                        StatusLabel.Content = "Bağlı.";
                        StatusLabel.Foreground = Brushes.Green;
                    }
                    else
                    {
                        StatusLabel.Content = "Bağlanmadı.";
                        StatusLabel.Foreground = Brushes.Red;
                    }
                }));
        }

        public MessageClientStatusView()
        {
            InitializeComponent();

            _timer = new Timer(OnTimerTick, null, Timeout.Infinite, 1000);
            if (LocalSettings.StartMessagingClient)
            {
                _timer.Change(10000, 10000);
            }
            else StatusLabel.Visibility = Visibility.Collapsed;
        }
    }
}
