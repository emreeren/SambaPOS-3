using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.ExceptionReporter;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public static class AppServices
    {
        public static Dispatcher MainDispatcher { get; set; }

        private static MainDataContext _mainDataContext;
        public static MainDataContext MainDataContext
        {
            get { return _mainDataContext ?? (_mainDataContext = new MainDataContext()); }
            set { _mainDataContext = value; }
        }

        private static MessagingService _messagingService;
        public static MessagingService MessagingService
        {
            get { return _messagingService ?? (_messagingService = new MessagingService()); }
        }

        private static SettingService _settingService;
        public static SettingService SettingService
        {
            get { return _settingService ?? (_settingService = new SettingService()); }
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

        public static void ResetCache()
        {
        
            MainDataContext.ResetCache();
            SettingService.ResetCache();
            SerialPortService.ResetCache();
            Dao.ResetCache();
            
        }
    }
}
