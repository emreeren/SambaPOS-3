using System.Collections.Generic;
using Samba.Domain.Models.Resources;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IResourceService : IService
    {
        IEnumerable<ResourceScreenItem> GetCurrentResourceScreenItems(ResourceScreen resourceScreen, int currentPageNo, int resourceStateFilter);
        IEnumerable<Resource> GetResourcesByState(int resourceStateId, int resourceTypeId);
        IList<ResourceScreenItem> LoadResourceScreenItems(string selectedResourceScreen);
        IList<Widget> LoadWidgets(string selectedResourceScreen);
        void SaveResourceScreenItems();
        int GetResourceScreenItemCount();
        IEnumerable<string> GetCategories();
        void UpdateResourceState(int resourceId, int stateId);
        void AddWidgetToResourceScreen(string resourceScreenName, Widget widget);
        void UpdateResourceScreen(ResourceScreen resourceScreen);
        void RemoveWidget(Widget widget);
        List<Resource> SearchResources(string searchString, ResourceType selectedResourceType, int stateFilter);
    }
}
