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
        private readonly IApplicationState _applicationState;
        private readonly IList<Resource> _emptyResourceList = new List<Resource>().AsReadOnly();

        [ImportingConstructor]
        public ResourceService(IApplicationState applicationState)
        {
            _resourceScreenItemCount = Dao.Count<ResourceScreenItem>();
            _applicationState = applicationState;

            ValidatorRegistry.RegisterDeleteValidator<Resource>(x => Dao.Exists<TicketResource>(y => y.ResourceId == x.Id), Resources.Resource, Resources.Ticket);
            ValidatorRegistry.RegisterDeleteValidator<ResourceTemplate>(x => Dao.Exists<Resource>(y => y.ResourceTemplateId == x.Id), Resources.ResourceTemplate, Resources.Resource);
            ValidatorRegistry.RegisterDeleteValidator<ResourceScreenItem>(x => Dao.Exists<ResourceScreen>(y => y.ScreenItems.Any(z => z.Id == x.Id)), Resources.Location, Resources.ResourceScreen);
            ValidatorRegistry.RegisterDeleteValidator<ResourceScreen>(x => Dao.Exists<Department>(y => y.ResourceScreens.Any(z => z.Id == x.Id)), Resources.ResourceScreen, Resources.Department);
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Resource>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Resource)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<ResourceTemplate>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.ResourceTemplate)));
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

            var selectedResourceScreen = _applicationState.SelectedResourceScreen;

            if (selectedResourceScreen != null)
            {
                if (selectedResourceScreen.PageCount > 1)
                {
                    return selectedResourceScreen.ScreenItems
                         .OrderBy(x => x.Order)
                         .Where(x => x.ResourceStateId == resourceStateFilter || resourceStateFilter == 0)
                         .Skip(selectedResourceScreen.ItemCountPerPage * currentPageNo)
                         .Take(selectedResourceScreen.ItemCountPerPage);
                }
                return selectedResourceScreen.ScreenItems.Where(x => x.ResourceStateId == resourceStateFilter || resourceStateFilter == 0);
            }
            return new List<ResourceScreenItem>();
        }

        public IEnumerable<Resource> GetResourcesByState(int resourceStateId, int resourceTemplateId)
        {
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var ids = w.Queryable<ResourceStateValue>().GroupBy(x => x.ResoruceId).Select(x => x.Max(y => y.Id));
                var vids = w.Queryable<ResourceStateValue>().Where(x => ids.Contains(x.Id) && (x.StateId == resourceStateId)).Select(x => x.ResoruceId).ToList();
                if (vids.Count > 0)
                    return w.Queryable<Resource>().Where(x => x.ResourceTemplateId == resourceTemplateId && vids.Contains(x.Id)).ToList();
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
