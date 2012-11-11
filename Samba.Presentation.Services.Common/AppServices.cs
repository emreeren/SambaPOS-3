using System;
using System.Windows;
using System.Windows.Threading;
using Samba.Infrastructure.ExceptionReporter;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;

namespace Samba.Presentation.Services.Common
{
    public static class AppServices
    {
        public static Dispatcher MainDispatcher { get; set; }

        private static MessagingService _messagingService;
        public static MessagingService MessagingService
        {
            get { return _messagingService ?? (_messagingService = new MessagingService()); }
        }

        public static bool CanStartApplication()
        {
            return LocalSettings.CurrentDbVersion <= 0 || LocalSettings.CurrentDbVersion == LocalSettings.DbVersion;
        }

        public static void LogError(Exception e)
        {
            MessageBox.Show(Resources.ErrorLogMessage + e.Message, Resources.Information, MessageBoxButton.OK, MessageBoxImage.Stop);
            Logger.Log(e);
        }

        public static void LogError(Exception e, string userMessage)
        {
            MessageBox.Show(userMessage, Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            Logger.Log(e);
        }

        public static void Log(string message)
        {
            Logger.Log(message);
        }
    }
}
