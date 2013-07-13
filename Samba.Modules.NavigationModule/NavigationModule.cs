using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.NavigationModule
{
    [ModuleExport(typeof(NavigationModule))]
    public class NavigationModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly NavigationView _navigationView;
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public NavigationModule(IRegionManager regionManager, NavigationView navigationView, IUserService userService,
            IApplicationState applicationState)
            : base(regionManager, AppScreens.Navigation)
        {
            _regionManager = regionManager;
            _navigationView = navigationView;
            _userService = userService;
            _applicationState = applicationState;

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

            EventServiceFactory.EventService.GetEvent<GenericEvent<AppScreenChangeData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.Changed)
                    {
                        _applicationState.NotifyEvent(RuleEventNames.ApplicationScreenChanged,
                            new
                                {
                                    PreviousScreen = Enum.GetName(typeof(AppScreens), x.Value.PreviousScreen),
                                    CurrentScreen = Enum.GetName(typeof(AppScreens), x.Value.CurrentScreen)
                                });
                    }
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
            if (_userService.IsUserPermittedFor(PermissionNames.OpenNavigation))
            {
                Activate();
                ((NavigationViewModel)_navigationView.DataContext).Refresh();
            }
            else if (_applicationState.IsCurrentWorkPeriodOpen)
            {
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
            }
        }
    }
}
