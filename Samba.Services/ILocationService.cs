using System.Collections.Generic;
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
        string TestSaveOperation(Location model);
        string TestDeleteOperation(Location model);
        string TestDeleteOperation(LocationScreen model);
    }
}
