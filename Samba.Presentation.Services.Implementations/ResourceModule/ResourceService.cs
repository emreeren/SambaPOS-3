using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;
using Samba.Persistance.DaoClasses;
using Samba.Persistance.Data;

namespace Samba.Presentation.Services.Implementations.ResourceModule
{
    [Export(typeof(IResourceService))]
    public class ResourceService : IResourceService
    {
        private readonly IResourceDao _resourceDao;
        private IWorkspace _resoureceWorkspace;

        [ImportingConstructor]
        public ResourceService(IResourceDao resourceDao)
        {
            _resourceDao = resourceDao;
        }

        public void UpdateResourceScreenItems(ResourceScreen resourceScreen, int pageNo)
        {
            _resourceDao.UpdateResourceScreenItems(resourceScreen, pageNo);
        }

        public IEnumerable<ResourceScreenItem> GetCurrentResourceScreenItems(ResourceScreen resourceScreen, int currentPageNo, int resourceStateFilter)
        {
            UpdateResourceScreenItems(resourceScreen, currentPageNo);

            if (resourceScreen != null)
            {
                if (resourceScreen.PageCount > 1)
                {
                    return resourceScreen.ScreenItems
                         .OrderBy(x => x.Order)
                         .Where(x => x.ResourceStateId == resourceStateFilter || resourceStateFilter == 0)
                         .Skip(resourceScreen.ItemCountPerPage * currentPageNo)
                         .Take(resourceScreen.ItemCountPerPage);
                }
                return resourceScreen.ScreenItems.Where(x => x.ResourceStateId == resourceStateFilter || resourceStateFilter == 0);
            }
            return new List<ResourceScreenItem>();
        }

        public IEnumerable<Resource> GetResourcesByState(int resourceStateId, int resourceTypeId)
        {
            return _resourceDao.GetResourcesByState(resourceStateId, resourceTypeId);
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

        public List<Resource> SearchResources(string searchString, ResourceType selectedResourceType, int stateFilter)
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

        public void UpdateResourceState(int resourceId, int stateId)
        {
            _resourceDao.UpdateResourceState(resourceId, stateId);
        }
    }
}
