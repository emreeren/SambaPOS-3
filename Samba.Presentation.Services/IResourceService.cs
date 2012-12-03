using System.Collections.Generic;
using Samba.Domain.Models.Resources;

namespace Samba.Presentation.Services
{
    public interface IResourceService
    {
        IEnumerable<ResourceScreenItem> GetCurrentResourceScreenItems(ResourceScreen resourceScreen, int currentPageNo, string resourceStateFilter);
        IEnumerable<Resource> GetResourcesByState(string state, int resourceTypeId);
        IList<ResourceScreenItem> LoadResourceScreenItems(string selectedResourceScreen);
        IList<Widget> LoadWidgets(string selectedResourceScreen);
        void SaveResourceScreenItems();
        void UpdateResourceState(int resourceId, string stateName, string state);
        void AddWidgetToResourceScreen(string resourceScreenName, Widget widget);
        void UpdateResourceScreen(ResourceScreen resourceScreen);
        void RemoveWidget(Widget widget);
        List<Resource> SearchResources(string searchString, ResourceType selectedResourceType, string stateFilter);
    }
}
