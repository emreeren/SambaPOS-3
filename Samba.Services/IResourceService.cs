using System.Collections.Generic;
using Samba.Domain.Models.Resources;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IResourceService : IService
    {
        void UpdateResourceScreenItems(ResourceScreen resourceScreen, int pageNo);
        IEnumerable<ResourceScreenItem> GetCurrentResourceScreenItems(ResourceScreen resourceScreen, int currentPageNo);
        IList<ResourceScreenItem> LoadResourceScreenItems(string selectedResourceScreen);
        void SaveResourceScreenItems();
        int GetResourceScreenItemCount();
        IEnumerable<string> GetCategories();
        string GetCustomData(Resource resource, string fieldName);
        void UpdateResourceState(int resourceId, int stateId);
    }
}
