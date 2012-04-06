using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IApplicationStateSetter _applicationStateSetter;

        [ImportingConstructor]
        public ResourceService(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter)
        {
            _resourceScreenItemCount = Dao.Count<ResourceScreenItem>();
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;

            ValidatorRegistry.RegisterDeleteValidator(new ResourceDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new ResourceTemplateDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new ResourceScreenItemDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new ResourceScreenDeleteValidator());
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Resource>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Account)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<ResourceTemplate>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.AccountTemplate)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<ResourceScreenItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Location)));
        }

        public void UpdateResourceScreenItems(ResourceScreen resourceScreen, int pageNo)
        {
            _applicationStateSetter.SetSelectedResourceScreen(resourceScreen);

            if (resourceScreen != null)
            {
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
        }

        public IEnumerable<ResourceScreenItem> GetCurrentResourceScreenItems(ResourceScreen resourceScreen, int currentPageNo)
        {
            UpdateResourceScreenItems(resourceScreen, currentPageNo);

            var selectedResourceScreen = _applicationState.SelectedResourceScreen;

            if (selectedResourceScreen != null)
            {
                if (selectedResourceScreen.PageCount > 1)
                {
                    return selectedResourceScreen.ScreenItems
                         .OrderBy(x => x.Order)
                         .Skip(selectedResourceScreen.ItemCountPerPage * currentPageNo)
                         .Take(selectedResourceScreen.ItemCountPerPage);
                }
                return selectedResourceScreen.ScreenItems;
            }
            return new List<ResourceScreenItem>();
        }
        
        public IList<ResourceScreenItem> LoadResourceScreenItems(string selectedLocationScreen)
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
            }
            _resoureceWorkspace = WorkspaceFactory.Create();
            return _resoureceWorkspace.Single<ResourceScreen>(x => x.Name == selectedLocationScreen).ScreenItems;
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

        public string GetCustomData(Resource resource, string fieldName)
        {
            var pattern = string.Format("\"Name\":\"{0}\",\"Value\":\"([^\"]+)\"", fieldName);
            return Regex.IsMatch(resource.CustomData, pattern)
                ? Regex.Match(resource.CustomData, pattern).Groups[1].Value : "";
        }

        public void UpdateResourceState(int resourceId, int stateId)
        {
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

    public class ResourceDeleteValidator : SpecificationValidator<Resource>
    {
        public override string GetErrorMessage(Resource model)
        {
            if (Dao.Exists<TicketResource>(x => x.ResourceId== model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Resource, Resources.Ticket);
            return "";
        }
    }

    public class ResourceTemplateDeleteValidator : SpecificationValidator<ResourceTemplate>
    {
        public override string GetErrorMessage(ResourceTemplate model)
        {
            if (Dao.Exists<Resource>(x => x.ResourceTemplateId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.ResourceTemplate, Resources.Resource);
            return "";
        }
    }

    internal class ResourceScreenItemDeleteValidator : SpecificationValidator<ResourceScreenItem>
    {
        public override string GetErrorMessage(ResourceScreenItem model)
        {
            if (Dao.Exists<ResourceScreen>(x => x.ScreenItems.Any(y => y.Id == model.Id)))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Location, Resources.LocationScreen);
            return "";
        }
    }

    internal class ResourceScreenDeleteValidator : SpecificationValidator<ResourceScreen>
    {
        public override string GetErrorMessage(ResourceScreen model)
        {
            if (Dao.Exists<Department>(x => x.LocationScreens.Any(y => y.Id == model.Id)))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.LocationScreen, Resources.Department);
            return "";
        }
    }
}
