using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Locations;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.LocationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class LocationListViewModel : EntityCollectionViewModelBase<LocationEditorViewModel, Location>
    {
        private readonly ILocationService _locationService;
        public ICaptionCommand BatchCreateLocations { get; set; }

        [ImportingConstructor]
        public LocationListViewModel(ILocationService locationService)
        {
            _locationService = locationService;
            BatchCreateLocations = new CaptionCommand<string>(Resources.AddMultipleLocations, OnBatchCreateLocationsExecute);
            CustomCommands.Add(BatchCreateLocations);
        }

        private void OnBatchCreateLocationsExecute(string obj)
        {
            var values = InteractionService.UserIntraction.GetStringFromUser(
                Resources.AddMultipleLocations,
                Resources.AddMultipleLocationHint);

            var createdItems = new DataCreationService().BatchCreateLocations(values, Workspace);
            Workspace.CommitChanges();

            foreach (var location in createdItems)
                Items.Add(CreateNewViewModel(location));
        }

        protected override string CanDeleteItem(Location model)
        {
            var errors = _locationService.TestDeleteOperation(model);
            return !string.IsNullOrEmpty(errors) ? errors : base.CanDeleteItem(model);
        }
    }
}
