using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    class RecipeListViewModel : EntityCollectionViewModelBase<RecipeViewModel, Recipe>
    {
        protected override RecipeViewModel CreateNewViewModel(Recipe model)
        {
            return new RecipeViewModel(model);
        }

        protected override Recipe CreateNewModel()
        {
            return new Recipe();
        }
    }
}
