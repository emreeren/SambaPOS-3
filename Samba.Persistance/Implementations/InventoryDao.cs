using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Validation;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;

namespace Samba.Persistance.Implementations
{
    [Export(typeof(IInventoryDao))]
    class InventoryDao : IInventoryDao
    {
        [ImportingConstructor]
        public InventoryDao()
        {
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<InventoryItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.InventoryItem)));
            ValidatorRegistry.RegisterDeleteValidator<InventoryItem>(x => Dao.Exists<PeriodicConsumptionItem>(y => y.InventoryItemId == x.Id), Resources.InventoryItem, Resources.EndOfDayRecord);
            ValidatorRegistry.RegisterSaveValidator(new RecipeSaveValidator());
        }

        public IEnumerable<InventoryTransaction> GetTransactionItems(DateTime workPeriodStartDate, int inventoryItemId, int warehouseId)
        {
            return Dao.Query<InventoryTransaction>(x => x.InventoryItem.Id == inventoryItemId && x.Date > workPeriodStartDate && (x.TargetWarehouseId == warehouseId || x.SourceWarehouseId == warehouseId), x => x.InventoryItem);

            //return Dao.Query<InventoryTransactionDocument>(x =>
            //                                       x.Date > workPeriodStartDate
            //                                       && (x.TargetWarehouseId == warehouseId || x.SourceWarehouseId == warehouseId)
            //                                       && x.TransactionItems.Any(y => y.InventoryItem.Id == inventoryItemId),
            //                                       x => x.TransactionItems.Select(y => y.InventoryItem))
            //          .SelectMany(x => x.TransactionItems)
            //          .Where(x => x.InventoryItem.Id == inventoryItemId);
        }

        public IEnumerable<InventoryTransaction> GetTransactionItems(DateTime workPeriodStartDate, int warehouseId)
        {
            return Dao.Query<InventoryTransaction>(x => x.Date > workPeriodStartDate && (x.TargetWarehouseId == warehouseId || x.SourceWarehouseId == warehouseId), x => x.InventoryItem);

            //return Dao.Query<InventoryTransactionDocument>(x =>
            //                                       x.Date > workPeriodStartDate
            //                                       && (x.TargetWarehouseId == warehouseId || x.SourceWarehouseId == warehouseId),
            //                                       x => x.TransactionItems.Select(y => y.InventoryItem))
            //          .SelectMany(x => x.TransactionItems);
        }

        public IEnumerable<Order> GetOrdersFromRecipes(DateTime startDate, int inventoryItemId, int warehouseId)
        {
            var recipeItemIds = Dao.Select<Recipe, int>(x => x.Portion.MenuItemId, x => x.Portion != null && x.RecipeItems.Any(y => y.InventoryItem.Id == inventoryItemId)).Distinct();
            var tickets = Dao.Query<Ticket>(x => x.Date > startDate, x => x.Orders);
            return tickets.SelectMany(x => x.Orders)
                    .Where(x => (x.DecreaseInventory || x.IncreaseInventory) && x.WarehouseId == warehouseId && recipeItemIds.Contains(x.MenuItemId));
        }

        public IEnumerable<Order> GetOrdersFromRecipes(DateTime startDate, int warehouseId)
        {
            var recipeItemIds = Dao.Select<Recipe, int>(x => x.Portion.MenuItemId, x => x.Portion != null).Distinct();
            var tickets = Dao.Query<Ticket>(x => x.Date > startDate, x => x.Orders);
            return tickets.SelectMany(x => x.Orders)
                    .Where(x => (x.DecreaseInventory || x.IncreaseInventory) && x.WarehouseId == warehouseId && recipeItemIds.Contains(x.MenuItemId));
        }

        public Recipe GetRecipe(string portionName, int menuItemId)
        {
            return Dao.Single<Recipe>(x => x.Portion.Name == portionName && x.Portion.MenuItemId == menuItemId, x => x.Portion, x => x.RecipeItems, x => x.RecipeItems.Select(y => y.InventoryItem));
        }

        public IEnumerable<string> GetGroupCodes()
        {
            return Dao.Distinct<InventoryItem>(x => x.GroupCode);
        }

        public IEnumerable<string> GetInventoryItemNames()
        {
            return Dao.Select<InventoryItem, string>(x => x.Name, x => !string.IsNullOrEmpty(x.Name));
        }

        public bool RecipeExists()
        {
            return Dao.Exists<Recipe>();
        }

        public IEnumerable<InventoryItem> GetInventoryItems()
        {
            return Dao.Query<InventoryItem>();
        }

        public PeriodicConsumption GetPeriodicConsumptionByWorkPeriodId(int workPeriodId)
        {
            return Dao.Single<PeriodicConsumption>(x => x.WorkPeriodId == workPeriodId, x => x.WarehouseConsumptions.Select(y => y.PeriodicConsumptionItems), x => x.WarehouseConsumptions.Select(y => y.CostItems));
        }

        public PeriodicConsumptionItem GetPeriodConsumptionItem(int workPeriodId, int inventoryItemId, int warehouseId)
        {
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var pcId = w.Queryable<PeriodicConsumption>().Where(y => y.WorkPeriodId == workPeriodId).Select(y => y.Id);
                var wcId = w.Queryable<WarehouseConsumption>().Where(x => x.WarehouseId == warehouseId && x.PeriodicConsumptionId == pcId.FirstOrDefault()).Select(x => x.Id);
                var pci = w.Single<PeriodicConsumptionItem>(x => x.InventoryItemId == inventoryItemId && x.WarehouseConsumptionId == wcId.FirstOrDefault());
                return pci;
            }
        }

        public IEnumerable<string> GetWarehouseNames()
        {
            return Dao.Select<Warehouse, string>(x => x.Name, x => x.Id > 0);
        }
    }

    public class RecipeSaveValidator : SpecificationValidator<Recipe>
    {
        public override string GetErrorMessage(Recipe model)
        {
            if (model.RecipeItems.Any(x => x.InventoryItem == null || x.Quantity == 0))
                return Resources.SaveErrorZeroOrNullInventoryLines;
            if (model.Portion == null)
                return Resources.APortionShouldSelected;
            if (Dao.Exists<Recipe>(x => x.Portion.Id == model.Portion.Id && x.Id != model.Id))
            {
                var recipeName = Dao.Select<Recipe, string>(x => x.Name,
                                                        x => x.Portion.Id == model.Portion.Id && x.Id != model.Id).First();
                var mitemName = Dao.Single<MenuItem, string>(model.Portion.MenuItemId, x => x.Name);
                return string.Format(Resources.ThereIsAnotherRecipeFor_f, string.Format("{0} {1} [{2}]", mitemName, model.Portion.Name, recipeName));
            }
            return "";
        }
    }
}
