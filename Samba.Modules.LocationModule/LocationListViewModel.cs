using System.Linq;
using Samba.Domain.Models.Locations;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.LocationModule
{
    public class LocationListViewModel : EntityCollectionViewModelBase<LocationEditorViewModel, Location>
    {
        public ICaptionCommand BatchCreateLocations { get; set; }

        public LocationListViewModel()
        {
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

        protected override LocationEditorViewModel CreateNewViewModel(Location model)
        {
            return new LocationEditorViewModel(model);
        }

        protected override Location CreateNewModel()
        {
            return new Location();
        }

        protected override string CanDeleteItem(Location model)
        {
            if (model.TicketId > 0) return Resources.DeleteErrorTicketAssignedToLocation;
            var count = Dao.Count<LocationScreen>(x => x.Locations.Any(y => y.Id == model.Id));
            if (count > 0) return Resources.DeleteErrorLocationUsedInLocationView;
            return base.CanDeleteItem(model);
        }
    }
}
