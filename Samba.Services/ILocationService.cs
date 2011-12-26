using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Locations;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface ILocationService : IService
    {
        void UpdateLocations(LocationScreen locationScreen, int pageNo);
        IEnumerable<Location> GetCurrentLocations(LocationScreen locationScreen, int currentPageNo);
        IList<Location> LoadLocations(string selectedLocationScreen);
        int GetLocationCount();
        void SaveLocations();
        IEnumerable<string> GetCategories();
        Location GetLocationByModel(Location model);
        int GetLocationCountByLocationScreen(int locationId);
        bool DidLocationScreenUsedInDepartment(int id);
        string GetSaveErrorMessage(Location model);
    }
}
