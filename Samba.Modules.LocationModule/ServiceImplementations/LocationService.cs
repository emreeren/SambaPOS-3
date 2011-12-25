using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Locations;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.LocationModule.ServiceImplementations
{
    [Export(typeof(ILocationService))]
    public class LocationService : AbstractService, ILocationService
    {
        private IWorkspace _locationWorkspace;
        private readonly int _locationCount;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;

        [ImportingConstructor]
        public LocationService(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter)
        {
            _locationCount = Dao.Count<Location>(null);
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
        }

        public void UpdateLocations(LocationScreen locationScreen, int pageNo)
        {
            _applicationStateSetter.SetSelectedLocationScreen(locationScreen);

            if (locationScreen != null)
            {
                IEnumerable<int> set;
                if (locationScreen.PageCount > 1)
                {
                    set = locationScreen.Locations
                        .OrderBy(x => x.Order)
                        .Skip(pageNo * locationScreen.ItemCountPerPage)
                        .Take(locationScreen.ItemCountPerPage)
                        .Select(x => x.Id);
                }
                else set = locationScreen.Locations.OrderBy(x => x.Order).Select(x => x.Id);

                var result = Dao.Select<Location, dynamic>(x => new { x.Id, Tid = x.TicketId, Locked = x.IsTicketLocked },
                                                       x => set.Contains(x.Id));

                result.ToList().ForEach(x =>
                {
                    var location = locationScreen.Locations.Single(y => y.Id == x.Id);
                    location.TicketId = x.Tid;
                    location.IsTicketLocked = x.Locked;
                });
            }
        }

        public IEnumerable<Location> GetCurrentLocations(LocationScreen locationScreen, int currentPageNo)
        {
            UpdateLocations(locationScreen, currentPageNo);

            var selectedLocationScreen = _applicationState.SelectedLocationScreen;

            if (selectedLocationScreen != null)
            {
                if (selectedLocationScreen.PageCount > 1)
                {
                    return selectedLocationScreen.Locations
                         .OrderBy(x => x.Order)
                         .Skip(selectedLocationScreen.ItemCountPerPage * currentPageNo)
                         .Take(selectedLocationScreen.ItemCountPerPage);
                }
                return selectedLocationScreen.Locations;
            }
            return new List<Location>();
        }


        public IList<Location> LoadLocations(string selectedLocationScreen)
        {
            if (_locationWorkspace != null)
            {
                _locationWorkspace.CommitChanges();
            }
            _locationWorkspace = WorkspaceFactory.Create();
            return _locationWorkspace.Single<LocationScreen>(x => x.Name == selectedLocationScreen).Locations;
        }

        public int GetLocationCount()
        {
            return _locationCount;
        }

        public void SaveLocations()
        {
            if (_locationWorkspace != null)
            {
                _locationWorkspace.CommitChanges();
                _locationWorkspace = null;
            }
        }

        public override void Reset()
        {

        }
    }
}
