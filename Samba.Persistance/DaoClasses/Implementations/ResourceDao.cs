using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.DaoClasses.Implementations
{
    [Export(typeof(IResourceDao))]
    class ResourceDao : IResourceDao
    {
        [ImportingConstructor]
        public ResourceDao()
        {
            ValidatorRegistry.RegisterDeleteValidator<Resource>(x => Dao.Exists<TicketResource>(y => y.ResourceId == x.Id), Resources.Resource, Resources.Ticket);
            ValidatorRegistry.RegisterDeleteValidator<ResourceType>(x => Dao.Exists<Resource>(y => y.ResourceTypeId == x.Id), Resources.ResourceType, Resources.Resource);
            ValidatorRegistry.RegisterDeleteValidator<ResourceScreenItem>(x => Dao.Exists<ResourceScreen>(y => y.ScreenItems.Any(z => z.Id == x.Id)), Resources.ResourceScreenItem, Resources.ResourceScreen);
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Resource>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Resource)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<ResourceType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.ResourceType)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<ResourceScreenItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.ResourceScreenItem)));
        }

        private readonly IList<Resource> _emptyResourceList = new List<Resource>().AsReadOnly();

        public void UpdateResourceScreenItems(ResourceScreen resourceScreen, int pageNo)
        {
            if (resourceScreen == null) return;

            IEnumerable<int> set;
            if (resourceScreen.PageCount > 1)
            {
                set = resourceScreen.ScreenItems
                    .OrderBy(x => x.SortOrder)
                    .Skip(pageNo * resourceScreen.ItemCountPerPage)
                    .Take(resourceScreen.ItemCountPerPage)
                    .Select(x => x.ResourceId);
            }
            else set = resourceScreen.ScreenItems.OrderBy(x => x.SortOrder).Select(x => x.ResourceId);
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var result = w.Queryable<ResourceStateValue>().Where(x => set.Contains(x.ResoruceId));
                result.ToList().ForEach(x =>
                {
                    var screeenItem = resourceScreen.ScreenItems.Single(y => y.ResourceId == x.ResoruceId);
                    screeenItem.ResourceState = x.GetStateValue(resourceScreen.DisplayState);
                });
            }
        }

        public IEnumerable<Resource> GetResourcesByState(string state, int resourceTypeId)
        {
            var sv = string.Format("\"S\":\"{0}\"", state);
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var ids = w.Queryable<ResourceStateValue>().GroupBy(x => x.ResoruceId).Select(x => x.Max(y => y.Id));
                var vids = w.Queryable<ResourceStateValue>().Where(x => ids.Contains(x.Id) && (x.ResourceStates.Contains(sv))).Select(x => x.ResoruceId).ToList();
                if (vids.Count > 0)
                    return w.Queryable<Resource>().Where(x => x.ResourceTypeId == resourceTypeId && vids.Contains(x.Id)).ToList();
                return _emptyResourceList;
            }
        }

        public List<Resource> FindResources(ResourceType resourceType, string searchString, string stateFilter)
        {
            var templateId = resourceType != null ? resourceType.Id : 0;
            var searchValue = searchString.ToLower();

            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var result =
                    w.Query<Resource>(
                        x =>
                        x.ResourceTypeId == templateId &&
                        (x.CustomData.Contains(searchString) || x.Name.ToLower().Contains(searchValue))).Take(250).ToList();

                if (resourceType != null)
                    result = result.Where(x => resourceType.GetMatchingFields(x, searchString).Any(y => !y.Hidden) || x.Name.ToLower().Contains(searchValue)).ToList();

                if (!string.IsNullOrEmpty(stateFilter))
                {
                    var sv = string.Format("\"S\":\"{0}\"", stateFilter);
                    var set = result.Select(x => x.Id).ToList();
                    var ids = w.Queryable<ResourceStateValue>().Where(x => set.Contains(x.ResoruceId) && x.ResourceStates.Contains(sv)).GroupBy(x => x.ResoruceId).Select(x => x.Max(y => y.Id));
                    var resourceIds = w.Queryable<ResourceStateValue>().Where(x => ids.Contains(x.Id)).Select(x => x.ResoruceId).ToList();
                    result = result.Where(x => resourceIds.Contains(x.Id)).ToList();
                }
                return result;
            }
        }

        public void UpdateResourceState(int resourceId, string stateName, string state)
        {
            if (resourceId == 0) return;
            using (var w = WorkspaceFactory.Create())
            {
                var stateValue = w.Single<ResourceStateValue>(x => x.ResoruceId == resourceId);
                if (stateValue == null)
                {
                    stateValue = new ResourceStateValue { ResoruceId = resourceId };
                    w.Add(stateValue);
                }
                stateValue.SetStateValue(stateName, state);
                w.CommitChanges();
            }
        }

        public Resource GetResourceById(int id)
        {
            return Dao.Single<Resource>(x => x.Id == id);
        }
    }
}
