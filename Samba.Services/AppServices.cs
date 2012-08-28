using System;
using System.Windows;
using System.Windows.Threading;
using Samba.Infrastructure.ExceptionReporter;
using Samba.Infrastructure.Settings;

namespace Samba.Services
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
            MessageBox.Show("Bir sorun tespit ettik.\r\n\r\nProgram çalışmaya devam edecek ancak en kısa zamanda teknik destek almanız önerilir. Lütfen teknik destek için program danışmanınız ile irtibat kurunuz.\r\n\r\nMesaj:\r\n" + e.Message, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Stop);
            Logger.Log(e);
        }

        public static void LogError(Exception e, string userMessage)
        {
            MessageBox.Show(userMessage, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            Logger.Log(e);
        }

        public static void Log(string message)
        {
            Logger.Log(message);
        }
    }
}
