using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface ILocationService : IService
    {
        void UpdateLocations(ResourceScreen locationScreen, int pageNo);
        IEnumerable<ResourceScreenItem> GetCurrentLocations(ResourceScreen locationScreen, int currentPageNo);
        IList<ResourceScreenItem> LoadLocations(string selectedLocationScreen);
        int GetLocationCount();
        void SaveLocations();
        IEnumerable<string> GetCategories();
    }
}
