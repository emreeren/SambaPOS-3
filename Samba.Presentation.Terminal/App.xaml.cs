using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Samba.Infrastructure;

namespace Samba.Presentation.Terminal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (MessagingClient.IsConnected)
                MessagingClient.Disconnect();
        }
    }
}
