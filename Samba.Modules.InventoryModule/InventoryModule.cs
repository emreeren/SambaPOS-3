using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.InventoryModule
{
    [ModuleExport(typeof(InventoryModule))]
    public class InventoryModule : ModuleBase
    {
        [ImportingConstructor]
        public InventoryModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<WarehouseTypeViewModel, WarehouseType>>(Resources.WarehouseType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<WarehouseViewModel, Warehouse>>(Resources.Warehouse.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<InventoryItemViewModel, InventoryItem>>(Resources.InventoryItems, Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<RecipeViewModel, Recipe>>(Resources.Recipes, Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TransactionTypeViewModel, InventoryTransactionType>>(Resources.TransactionType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<TransactionListViewModel>(Resources.Transaction.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<PeriodicConsumptionListViewModel>(Resources.EndOfDayRecords, Resources.Inventory, 36);
        }
    }
}
