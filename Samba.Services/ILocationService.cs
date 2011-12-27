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
        bool DidLocationScreenUsedInDepartment(int id);
        OperationTestResult TestSaveOperation(Location model);
        OperationTestResult TestDeleteOperation(Location model);
    }
}
