using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Samba.Infrastructure.Settings;
using Samba.Modules.EntityModule;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Kiosk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var screenName = e.Args.FirstOrDefault() ?? "Kiosk";
            base.OnStartup(e);
            MefBootstrapper.ComposeParts();
            var applicationStateSetter = MefBootstrapper.Resolve<IApplicationStateSetter>();
            var cacheService = MefBootstrapper.Resolve<ICacheService>();
            foreach (var registeredCreator in MefBootstrapper.ResolveMany<IWidgetCreator>())
            {
                WidgetCreatorRegistry.RegisterWidgetCreator(registeredCreator);
            }
            var entityDashboardViewModel = MefBootstrapper.Resolve<EntityDashboardViewModel>();
            var selectedEntityScreen = cacheService.GetEntityScreenByName(screenName);
            applicationStateSetter.SetSelectedEntityScreen(selectedEntityScreen);
            var mainWindow = new MainWindow
                                 {
                                     DataContext = entityDashboardViewModel
                                 };
            entityDashboardViewModel.Refresh(selectedEntityScreen, null);

            var messagingService = MefBootstrapper.Resolve<IMessagingService>();
            messagingService.RegisterMessageListener(new MessageListener());

            if (LocalSettings.StartMessagingClient)
                messagingService.StartMessagingClient();

            mainWindow.Show();
        }
    }
}
