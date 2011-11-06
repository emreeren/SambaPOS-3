using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.NavigationModule
{
    [ModuleExport(typeof(NavigationModule))]
    public class NavigationModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly NavigationView _navigationView;
        
        [ImportingConstructor]
        public NavigationModule(IRegionManager regionManager, NavigationView navigationView)
            : base(regionManager, AppScreens.Navigation)
        {
            _regionManager = regionManager;
            _navigationView = navigationView;

            PermissionRegistry.RegisterPermission(PermissionNames.OpenNavigation, PermissionCategories.Navigation, Resources.CanOpenNavigation);

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.UserLoggedIn)
                        ActivateNavigation();
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateNavigation)
                        ActivateNavigation();
                });
        }

        public override object GetVisibleView()
        {
            return _navigationView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(NavigationView));
        }

        private void ActivateNavigation()
        {
            if (AppServices.IsUserPermittedFor(PermissionNames.OpenNavigation))
            {
                Activate();
                ((NavigationViewModel)_navigationView.DataContext).Refresh();
            }
            else
            {
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
            }
        }
    }
}
