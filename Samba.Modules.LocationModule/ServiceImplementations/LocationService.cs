using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Locations;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.LocationModule.ServiceImplementations
{
    [Export(typeof(ILocationService))]
    public class LocationService : AbstractService, ILocationService
    {
        private IWorkspace _locationWorkspace;
        private readonly int _locationCount;

        public LocationService()
        {
            _locationCount = Dao.Count<Location>(null);
        }

        public LocationScreen SelectedLocationScreen { get; set; }

        public void UpdateLocations(LocationScreen locationScreen, int pageNo)
        {
            SelectedLocationScreen = locationScreen;
            if (SelectedLocationScreen != null)
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

            var selectedLocationScreen = SelectedLocationScreen;

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
