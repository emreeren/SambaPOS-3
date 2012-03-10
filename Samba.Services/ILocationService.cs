using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface ILocationService : IService
    {
        void UpdateLocations(AccountScreen locationScreen, int pageNo);
        IEnumerable<AccountScreenItem> GetCurrentLocations(AccountScreen locationScreen, int currentPageNo);
        IList<AccountScreenItem> LoadLocations(string selectedLocationScreen);
        int GetLocationCount();
        void SaveLocations();
        IEnumerable<string> GetCategories();
    }
}
