using System.ComponentModel.Composition;
using Samba.Domain.Models.Inventories;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class RecipeListViewModel : EntityCollectionViewModelBase<RecipeViewModel, Recipe>
    {

    }
}
