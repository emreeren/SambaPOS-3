using System;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.ServiceLocation;
using Samba.Infrastructure.Settings;
using Samba.Localization.Engine;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Presentation
{
    public class Bootstrapper : MefBootstrapper
    {
        static Mutex _mutex = new Mutex(true, "{aa77c8c4-b8c1-4e61-908f-48c6fb65227d}");

        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<Shell>();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
            var path = System.IO.Path.GetDirectoryName(Application.ResourceAssembly.Location);
            if (path != null)
            {
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Login*"));
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Modules*"));
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Presentation*"));
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Services.dll"));
            }
            LocalSettings.AppPath = path;
        }

        protected override void InitializeModules()
        {
            base.InitializeModules();
            var moduleInitializationService = ServiceLocator.Current.GetInstance<IModuleInitializationService>();
            moduleInitializationService.Initialize();
        }

        protected override void InitializeShell()
        {
            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWSAMBAPOS, IntPtr.Zero, IntPtr.Zero);
                Application.Current.Shutdown();
                return;
            }

            LocalizeDictionary.ChangeLanguage(LocalSettings.CurrentLanguage);

            InteractionService.UserIntraction = ServiceLocator.Current.GetInstance<IUserInteraction>();
            InteractionService.UserIntraction.ToggleSplashScreen();

            AppServices.MainDispatcher = Application.Current.Dispatcher;

            AppServices.MessagingService.RegisterMessageListener(new MessageListener());

            if (LocalSettings.StartMessagingClient)
                AppServices.MessagingService.StartMessagingClient();

            GenericRuleRegistator.RegisterOnce();

            PresentationServices.Initialize();

            base.InitializeShell();

            try
            {
                var creationService = new DataCreationService();
                creationService.CreateData();
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(LocalSettings.ConnectionString))
                {
                    var connectionString =
                        InteractionService.UserIntraction.GetStringFromUser(
                        "Connection String",
                        "Şu anki bağlantı ayarları ile veri tabanına bağlanılamıyor. Lütfen aşağıdaki bağlantı bilgisini kontrol ederek tekrar deneyiniz.\r\r" +
                        "Hata Mesajı:\r" + e.Message,
                        LocalSettings.ConnectionString);

                    var cs = String.Join(" ", connectionString);

                    if (!string.IsNullOrEmpty(cs))
                        LocalSettings.ConnectionString = cs.Trim();

                    AppServices.LogError(e, "Programı yeniden başlatınız. Mevcut problem log dosyasına kaydedildi.");
                }
                else
                {
                    AppServices.LogError(e);
                    LocalSettings.ConnectionString = "";
                }
                LocalSettings.SaveSettings();
                Environment.Exit(1);
            }

            Application.Current.MainWindow = (Shell)Shell;
            Application.Current.MainWindow.Show();
            AppServices.CurrentLoggedInUser.PublishEvent(EventTopicNames.UserLoggedOut);
            InteractionService.UserIntraction.ToggleSplashScreen();
            TriggerService.UpdateCronObjects();
        }
    }
    internal class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWSAMBAPOS = RegisterWindowMessage("WM_SHOWSAMBAPOS");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
}
