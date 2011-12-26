using System.ComponentModel.Composition;
using Samba.Domain.Models.Locations;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.LocationModule
{
    [Export,PartCreationPolicy(CreationPolicy.NonShared)]
    public class LocationScreenListViewModel : EntityCollectionViewModelBase<LocationScreenViewModel, LocationScreen>
    {
        private readonly ILocationService _locationService;

        [ImportingConstructor]
        public LocationScreenListViewModel(ILocationService locationService)
        {
            _locationService = locationService;
        }

        protected override string CanDeleteItem(LocationScreen model)
        {
            if (_locationService.DidLocationScreenUsedInDepartment(model.Id))
                return Resources.DeleteErrorLocationViewUsedInDepartment;
            return base.CanDeleteItem(model);
        }
    }
}
