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

        [ImportingConstructor]
        public LocationModule(IRegionManager regionManager, LocationSelectorView locationSelectorView)
            : base(regionManager, AppScreens.LocationList)
        {
            _regionManager = regionManager;
            _locationSelectorView = locationSelectorView;

            AddDashboardCommand<LocationListViewModel>(Resources.LocationList, Resources.Locations, 30);
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
