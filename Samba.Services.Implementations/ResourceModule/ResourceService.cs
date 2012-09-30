using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.ResourceModule
{
    [Export(typeof(IResourceService))]
    public class ResourceService : AbstractService, IResourceService
    {
        private IWorkspace _resoureceWorkspace;
        private readonly int _resourceScreenItemCount;
        private readonly IList<Resource> _emptyResourceList = new List<Resource>().AsReadOnly();

        [ImportingConstructor]
        public ResourceService()
        {
            _resourceScreenItemCount = Dao.Count<ResourceScreenItem>();

            ValidatorRegistry.RegisterDeleteValidator<Resource>(x => Dao.Exists<TicketResource>(y => y.ResourceId == x.Id), Resources.Resource, Resources.Ticket);
            ValidatorRegistry.RegisterDeleteValidator<ResourceType>(x => Dao.Exists<Resource>(y => y.ResourceTypeId == x.Id), Resources.ResourceType, Resources.Resource);
            ValidatorRegistry.RegisterDeleteValidator<ResourceScreenItem>(x => Dao.Exists<ResourceScreen>(y => y.ScreenItems.Any(z => z.Id == x.Id)), Resources.ResourceScreenItem, Resources.ResourceScreen);
            ValidatorRegistry.RegisterDeleteValidator<ResourceScreen>(x => Dao.Exists<Department>(y => y.ResourceScreens.Any(z => z.Id == x.Id)), Resources.ResourceScreen, Resources.Department);
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Resource>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Resource)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<ResourceType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.ResourceType)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<ResourceScreenItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.ResourceScreenItem)));
        }

        public void UpdateResourceScreenItems(ResourceScreen resourceScreen, int pageNo)
        {
            if (resourceScreen == null) return;

            IEnumerable<int> set;
            if (resourceScreen.PageCount > 1)
            {
                set = resourceScreen.ScreenItems
                    .OrderBy(x => x.Order)
                    .Skip(pageNo * resourceScreen.ItemCountPerPage)
                    .Take(resourceScreen.ItemCountPerPage)
                    .Select(x => x.ResourceId);
            }
            else set = resourceScreen.ScreenItems.OrderBy(x => x.Order).Select(x => x.ResourceId);

            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var ids = w.Queryable<ResourceStateValue>().Where(x => set.Contains(x.ResoruceId)).GroupBy(x => x.ResoruceId).Select(x => x.Max(y => y.Id));
                var result = w.Queryable<ResourceStateValue>().Where(x => ids.Contains(x.Id)).Select(x => new { AccountId = x.ResoruceId, x.StateId });
                result.ToList().ForEach(x =>
                                            {
                                                var location = resourceScreen.ScreenItems.Single(y => y.ResourceId == x.AccountId);
                                                location.ResourceStateId = x.StateId;
                                            });
            }
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
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var ids = w.Queryable<ResourceStateValue>().GroupBy(x => x.ResoruceId).Select(x => x.Max(y => y.Id));
                var vids = w.Queryable<ResourceStateValue>().Where(x => ids.Contains(x.Id) && (x.StateId == resourceStateId)).Select(x => x.ResoruceId).ToList();
                if (vids.Count > 0)
                    return w.Queryable<Resource>().Where(x => x.ResourceTypeId == resourceTypeId && vids.Contains(x.Id)).ToList();
                return _emptyResourceList;
            }
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
            var templateId = selectedResourceType != null ? selectedResourceType.Id : 0;
            var searchValue = searchString.ToLower();

            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var result =
                    w.Query<Resource>(
                        x =>
                        x.ResourceTypeId == templateId &&
                        (x.CustomData.Contains(searchString) || x.Name.ToLower().Contains(searchValue))).Take(250).ToList();

                if (selectedResourceType != null)
                    result = result.Where(x => selectedResourceType.GetMatchingFields(x, searchString).Any(y => !y.Hidden) || x.Name.ToLower().Contains(searchValue)).ToList();


                if (stateFilter > 0)
                {
                    var set = result.Select(x => x.Id).ToList();
                    var ids = w.Queryable<ResourceStateValue>().Where(x => set.Contains(x.ResoruceId) && x.StateId == stateFilter).GroupBy(x => x.ResoruceId).Select(x => x.Max(y => y.Id));
                    var resourceIds = w.Queryable<ResourceStateValue>().Where(x => ids.Contains(x.Id)).Select(x => x.ResoruceId).ToList();
                    result = result.Where(x => resourceIds.Contains(x.Id)).ToList();
                }
                return result;
            }
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

        public int GetResourceScreenItemCount()
        {
            return _resourceScreenItemCount;
        }

        public void SaveResourceScreenItems()
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
                _resoureceWorkspace = null;
            }
        }

        public IEnumerable<string> GetCategories()
        {
            return Dao.Distinct<ResourceScreenItem>(x => x.Category);
        }

        public void UpdateResourceState(int resourceId, int stateId)
        {
            if (resourceId == 0) return;
            using (var w = WorkspaceFactory.Create())
            {
                var csid = w.Last<ResourceStateValue>(x => x.ResoruceId == resourceId);
                if (csid == null || csid.StateId != stateId)
                {
                    var v = new ResourceStateValue { ResoruceId = resourceId, Date = DateTime.Now, StateId = stateId };
                    w.Add(v);
                    w.CommitChanges();
                }
            }
        }



        public override void Reset()
        {

        }
    }
}
