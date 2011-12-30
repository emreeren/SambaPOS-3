using System.ComponentModel.Composition;
using Samba.Domain.Models.Locations;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.LocationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class LocationScreenListViewModel : EntityCollectionViewModelBase<LocationScreenViewModel, LocationScreen>
    {
    }
}
