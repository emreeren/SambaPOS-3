using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;
using Samba.Persistance.DaoClasses;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.ResourceModule
{
    [Export(typeof(IResourceService))]
    public class ResourceService : IResourceService
    {
        private readonly IResourceDao _resourceDao;
        private readonly IAutomationService _automationService;
        private IWorkspace _resoureceWorkspace;

        [ImportingConstructor]
        public ResourceService(IResourceDao resourceDao, IAutomationService automationService)
        {
            _resourceDao = resourceDao;
            _automationService = automationService;
        }

        public void UpdateResourceScreenItems(ResourceScreen resourceScreen, int pageNo)
        {
            _resourceDao.UpdateResourceScreenItems(resourceScreen, pageNo);
        }

        public IEnumerable<ResourceScreenItem> GetCurrentResourceScreenItems(ResourceScreen resourceScreen, int currentPageNo, string resourceStateFilter)
        {
            UpdateResourceScreenItems(resourceScreen, currentPageNo);
            if (resourceScreen != null)
            {
                if (resourceScreen.PageCount > 1)
                {
                    return resourceScreen.ScreenItems
                         .OrderBy(x => x.Order)
                         .Where(x => string.IsNullOrEmpty(resourceStateFilter) || x.ResourceState == resourceStateFilter)
                         .Skip(resourceScreen.ItemCountPerPage * currentPageNo)
                         .Take(resourceScreen.ItemCountPerPage);
                }
                return resourceScreen.ScreenItems.Where(x => string.IsNullOrEmpty(resourceStateFilter) || x.ResourceState == resourceStateFilter);
            }
            return new List<ResourceScreenItem>();
        }

        public IEnumerable<Resource> GetResourcesByState(string state, int resourceTypeId)
        {
            return _resourceDao.GetResourcesByState(state, resourceTypeId);
        }

        public IList<Widget> LoadWidgets(string selectedResourceScreen)
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
            }
            _resoureceWorkspace = WorkspaceFactory.Create();
            return _resoureceWorkspace.Single<ResourceScreen>(x => x.Name == selectedResourceScreen).Widgets;
        }

        public void AddWidgetToResourceScreen(string resourceScreenName, Widget widget)
        {
            if (_resoureceWorkspace == null) return;
            _resoureceWorkspace.Single<ResourceScreen>(x => x.Name == resourceScreenName).Widgets.Add(widget);
            _resoureceWorkspace.CommitChanges();
        }

        public void UpdateResourceScreen(ResourceScreen resourceScreen)
        {
            UpdateResourceScreenItems(resourceScreen, 0);
        }

        public void RemoveWidget(Widget widget)
        {
            if (_resoureceWorkspace == null) return;
            _resoureceWorkspace.Delete<Widget>(x => x.Id == widget.Id);
            _resoureceWorkspace.CommitChanges();
        }

        public List<Resource> SearchResources(string searchString, ResourceType selectedResourceType, string stateFilter)
        {
            return _resourceDao.FindResources(selectedResourceType, searchString, stateFilter);
        }

        public IList<ResourceScreenItem> LoadResourceScreenItems(string selectedResourceScreen)
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
            }
            _resoureceWorkspace = WorkspaceFactory.Create();
            return _resoureceWorkspace.Single<ResourceScreen>(x => x.Name == selectedResourceScreen).ScreenItems;
        }

        public void SaveResourceScreenItems()
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
                _resoureceWorkspace = null;
            }
        }

        public void UpdateResourceState(int resourceId, string stateName, string state)
        {
            _resourceDao.UpdateResourceState(resourceId, stateName, state);
            _automationService.NotifyEvent(RuleEventNames.ResourceStateUpdated, new { ResourceId = resourceId, StateName = stateName, State = state });
        }
    }
}
