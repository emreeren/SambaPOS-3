using System.Linq;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.LocationModule
{
    public class LocationScreenListViewModel : EntityCollectionViewModelBase<LocationScreenViewModel, LocationScreen>
    {
        protected override string CanDeleteItem(LocationScreen model)
        {
            if (Dao.Query<Department>(x => x.LocationScreens.Any(y => y.Id == model.Id)) != null)
                return Resources.DeleteErrorLocationViewUsedInDepartment;
            return base.CanDeleteItem(model);
        }
    }
}
