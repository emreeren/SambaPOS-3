using System.ComponentModel.Composition;
using Samba.Domain.Models.Locations;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.LocationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
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
            var errors = _locationService.TestDeleteOperation(model);
            return !string.IsNullOrEmpty(errors) ? errors : base.CanDeleteItem(model);
        }
    }
}
