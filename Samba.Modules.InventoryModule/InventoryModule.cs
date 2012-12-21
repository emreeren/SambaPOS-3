using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    [ModuleExport(typeof(InventoryModule))]
    public class InventoryModule : ModuleBase
    {
        [ImportingConstructor]
        public InventoryModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<InventoryItemViewModel, InventoryItem>>(Resources.InventoryItems, Resources.Products, 26);
            AddDashboardCommand<EntityCollectionViewModelBase<RecipeViewModel, Recipe>>(Resources.Recipes, Resources.Products, 27);
            AddDashboardCommand<TransactionListViewModel>(Resources.Transactions, Resources.Products, 28);
            AddDashboardCommand<PeriodicConsumptionListViewModel>(Resources.EndOfDayRecords, Resources.Products, 29);
        }
    }
}
