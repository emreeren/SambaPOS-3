using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Locations;

namespace Samba.Services
{
    public interface ILocationService : IService
    {
        LocationScreen SelectedLocationScreen { get; set; }
        void UpdateLocations(LocationScreen locationScreen, int pageNo);
        IEnumerable<Location> GetCurrentLocations(LocationScreen locationScreen, int currentPageNo);
        IList<Location> LoadLocations(string selectedLocationScreen);
        int GetLocationCount();
        void SaveLocations();
    }
}
