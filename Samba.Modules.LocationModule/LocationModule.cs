using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.LocationModule
{
    [ModuleExport(typeof(LocationModule))]
    public class LocationModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly LocationSelectorView _locationSelectorView;
        private readonly LocationSelectorViewModel _locationSelectorViewModel;

        [ImportingConstructor]
        public LocationModule(IRegionManager regionManager, LocationSelectorView locationSelectorView,
            LocationSelectorViewModel locationSelectorViewModel)
            : base(regionManager, AppScreens.LocationList)
        {
            _regionManager = regionManager;
            _locationSelectorView = locationSelectorView;
            _locationSelectorViewModel = locationSelectorViewModel;

            AddDashboardCommand<EntityCollectionViewModelBase<LocationEditorViewModel, Location>>(string.Format(Resources.List_f, Resources.Location), Resources.Locations, 30);
            AddDashboardCommand<EntityCollectionViewModelBase<LocationScreenViewModel, LocationScreen>>(Resources.LocationViews, Resources.Locations);
        }

        public override object GetVisibleView()
        {
            return _locationSelectorView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(LocationSelectorView));

            PermissionRegistry.RegisterPermission(PermissionNames.OpenLocations, PermissionCategories.Navigation, Resources.CanOpenLocationList);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeLocation, PermissionCategories.Ticket, Resources.CanChangeLocation);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectLocation)
                {
                    _locationSelectorViewModel.SelectedTicket = null;
                    ActivateLocationView();
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectLocation)
                {
                    _locationSelectorViewModel.SelectedTicket = x.Value;
                    ActivateLocationView();
                }
            });
        }

        private void ActivateLocationView()
        {
            Activate();
        }
    }
}
