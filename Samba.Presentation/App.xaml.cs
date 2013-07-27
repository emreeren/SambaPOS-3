using System;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Samba.Infrastructure.Messaging;
using Samba.Presentation.Common.ErrorReport;
using Samba.Presentation.Services;
using Samba.Services.Common;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
            ServiceLocator.Current.GetInstance<IApplicationState>().NotifyEvent(RuleEventNames.ApplicationStarted, new { Arguments = string.Join(" ", e.Args) });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MessagingClient.Stop();
            ServiceLocator.Current.GetInstance<ITriggerService>().CloseTriggers();
        }

        private static void RunInDebugMode()
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        private static void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            try
            {
                var bootstrapper = new Bootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null) return;
            ExceptionReporter.Show(ex);
            Environment.Exit(1);
        }
    }
}
