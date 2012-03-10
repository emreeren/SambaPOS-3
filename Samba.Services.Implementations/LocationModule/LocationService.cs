using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.LocationModule
{
    [Export(typeof(ILocationService))]
    public class LocationService : AbstractService, ILocationService
    {
        private IWorkspace _locationWorkspace;
        private readonly int _locationCount;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;

        [ImportingConstructor]
        public LocationService(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter)
        {
            _locationCount = Dao.Count<AccountScreenItem>();
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;

            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountScreenItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Location)));
            ValidatorRegistry.RegisterDeleteValidator(new LocationDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new LocationScreenDeleteValidator());
        }

        public void UpdateLocations(AccountScreen locationScreen, int pageNo)
        {
            _applicationStateSetter.SetSelectedLocationScreen(locationScreen);

            if (locationScreen != null)
            {
                IEnumerable<int> set;
                if (locationScreen.PageCount > 1)
                {
                    set = locationScreen.ScreenItems
                        .OrderBy(x => x.Order)
                        .Skip(pageNo * locationScreen.ItemCountPerPage)
                        .Take(locationScreen.ItemCountPerPage)
                        .Select(x => x.Id);
                }
                else set = locationScreen.ScreenItems.OrderBy(x => x.Order).Select(x => x.Id);

                var result = Dao.Select<AccountScreenItem, dynamic>(
                    x =>
                        new { x.Id, Tid = x.TicketId, Locked = x.IsTicketLocked },
                        x => set.Contains(x.Id));

                result.ToList().ForEach(x =>
                {
                    var location = locationScreen.ScreenItems.Single(y => y.Id == x.Id);
                    location.TicketId = x.Tid;
                    location.IsTicketLocked = x.Locked;
                });
            }
        }

        public IEnumerable<AccountScreenItem> GetCurrentLocations(AccountScreen locationScreen, int currentPageNo)
        {
            UpdateLocations(locationScreen, currentPageNo);

            var selectedLocationScreen = _applicationState.SelectedLocationScreen;

            if (selectedLocationScreen != null)
            {
                if (selectedLocationScreen.PageCount > 1)
                {
                    return selectedLocationScreen.ScreenItems
                         .OrderBy(x => x.Order)
                         .Skip(selectedLocationScreen.ItemCountPerPage * currentPageNo)
                         .Take(selectedLocationScreen.ItemCountPerPage);
                }
                return selectedLocationScreen.ScreenItems;
            }
            return new List<AccountScreenItem>();
        }


        public IList<AccountScreenItem> LoadLocations(string selectedLocationScreen)
        {
            if (_locationWorkspace != null)
            {
                _locationWorkspace.CommitChanges();
            }
            _locationWorkspace = WorkspaceFactory.Create();
            return _locationWorkspace.Single<AccountScreen>(x => x.Name == selectedLocationScreen).ScreenItems;
        }

        public int GetLocationCount()
        {
            return _locationCount;
        }

        public void SaveLocations()
        {
            if (_locationWorkspace != null)
            {
                _locationWorkspace.CommitChanges();
                _locationWorkspace = null;
            }
        }

        public IEnumerable<string> GetCategories()
        {
            return Dao.Distinct<AccountScreenItem>(x => x.Category);
        }

        public override void Reset()
        {

        }
    }

    internal class LocationDeleteValidator : SpecificationValidator<AccountScreenItem>
    {
        public override string GetErrorMessage(AccountScreenItem model)
        {
            if (model.TicketId > 0) return Resources.DeleteErrorTicketAssignedToLocation;
            if (Dao.Exists<AccountScreen>(x => x.ScreenItems.Any(y => y.Id == model.Id)))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Location, Resources.LocationScreen);
            return "";
        }
    }

    internal class LocationScreenDeleteValidator : SpecificationValidator<AccountScreen>
    {
        public override string GetErrorMessage(AccountScreen model)
        {
            if (Dao.Exists<Department>(x => x.LocationScreens.Any(y => y.Id == model.Id)))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.LocationScreen, Resources.Department);
            return "";
        }
    }
}
