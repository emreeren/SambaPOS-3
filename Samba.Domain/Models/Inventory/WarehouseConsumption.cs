using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class WarehouseConsumption : ValueClass
    {
        public int PeriodicConsumptionId { get; set; }
        public int WarehouseId { get; set; }

        public WarehouseConsumption()
        {
            _periodicConsumptionItems = new List<PeriodicConsumptionItem>();
            _costItems = new List<CostItem>();
        }

        private IList<PeriodicConsumptionItem> _periodicConsumptionItems;
        public virtual IList<PeriodicConsumptionItem> PeriodicConsumptionItems
        {
            get { return _periodicConsumptionItems; }
            set { _periodicConsumptionItems = value; }
        }

        private IList<CostItem> _costItems;
        public virtual IList<CostItem> CostItems
        {
            get { return _costItems; }
            set { _costItems = value; }
        }

        private void UpdateConsumption(RecipeItem recipeItem, decimal saleTotal)
        {
            var pci = PeriodicConsumptionItems.Single(x => x.InventoryItemId == recipeItem.InventoryItem.Id);
            pci.Consumption += (recipeItem.Quantity * saleTotal) / pci.UnitMultiplier;
        }

        private decimal GetPredictedCost(RecipeItem recipeItem)
        {
            var pci = PeriodicConsumptionItems.Single(x => x.InventoryItemId == recipeItem.InventoryItem.Id);
            return recipeItem.Quantity * (pci.Cost / pci.UnitMultiplier);
        }

        private void UpdateFinalCost(Recipe recipe)
        {
            if (recipe == null) return;
            var ci = CostItems.SingleOrDefault(x => x.PortionId == recipe.Portion.Id);
            if (ci == null) return;
            var totalcost = recipe.FixedCost + recipe.GetValidRecipeItems().Sum(recipeItem => GetFinalCost(recipeItem));
            ci.Cost = decimal.Round(totalcost, 2);
        }

        private decimal GetFinalCost(RecipeItem recipeItem)
        {
            var pci = PeriodicConsumptionItems.SingleOrDefault(x => x.InventoryItemId == recipeItem.InventoryItem.Id);
            if (pci != null && pci.GetPredictedConsumption() > 0)
            {
                var cost = recipeItem.Quantity * (pci.Cost / pci.UnitMultiplier);
                cost = (pci.GetConsumption() * cost) / pci.GetPredictedConsumption();
                return cost;
            }
            return 0;
        }

        private void CreatePeriodicConsumptionItem(InventoryItem inventoryItem,
                                                   WarehouseConsumption previousWhc,
                                                   IEnumerable<InventoryTransaction> transactionItems)
        {
            var pci = PeriodicConsumptionItem.Create(inventoryItem);
            PeriodicConsumptionItems.Add(pci);
            var previousCost = 0m;
            if (previousWhc != null)
            {
                var previousPci = previousWhc.PeriodicConsumptionItems.SingleOrDefault(x => x.InventoryItemId == inventoryItem.Id);
                if (previousPci != null)
                    pci.InStock =
                        previousPci.PhysicalInventory != null
                            ? previousPci.PhysicalInventory.GetValueOrDefault(0)
                            : previousPci.GetInventoryPrediction();
                if (previousPci != null)
                    previousCost = previousPci.Cost * pci.InStock;
            }
            var tim = transactionItems.Where(x => x.InventoryItem.Id == inventoryItem.Id).ToList();
            pci.Added = tim.Where(x => x.TargetWarehouseId == WarehouseId).Sum(x => x.Quantity * x.Multiplier) / pci.UnitMultiplier;
            pci.Removed = tim.Where(x => x.SourceWarehouseId == WarehouseId).Sum(x => x.Quantity * x.Multiplier) / pci.UnitMultiplier;
            var totalPrice = tim.Where(x => x.TargetWarehouseId == WarehouseId).Sum(x => x.Price * x.Quantity);
            if (pci.InStock + pci.Purchase > 0)
                pci.Cost = decimal.Round((totalPrice + previousCost) / (pci.InStock + pci.Added), 2);

        }

        public void CreatePeriodicConsumptionItems(IEnumerable<InventoryItem> inventoryItems, WarehouseConsumption previousPc, List<InventoryTransaction> transactionItems)
        {
            foreach (var inventoryItem in inventoryItems)
            {
                CreatePeriodicConsumptionItem(inventoryItem, previousPc, transactionItems);
            }
        }

        public void CreateCostItem(Recipe recipe, string menuItemName, decimal saleTotal)
        {
            if (recipe == null) return;
            var recipeItems = recipe.GetValidRecipeItems().ToList();
            var totalCost = recipeItems.Sum(recipeItem => GetPredictedCost(recipeItem));

            CostItems.Add(new CostItem
                {
                    Name = menuItemName,
                    PortionId = recipe.Portion.Id,
                    MenuItemId = recipe.Portion.MenuItemId,
                    PortionName = recipe.Portion.Name,
                    CostPrediction = totalCost,
                    Quantity = saleTotal
                });
        }

        public void UpdateFinalCost(IEnumerable<Recipe> recipes)
        {
            recipes.ToList().ForEach(UpdateFinalCost);
        }

        public void UpdateConsumption(Recipe recipe, decimal saleTotal)
        {
            var recipeItems = recipe.GetValidRecipeItems().ToList();
            recipeItems.ForEach(x => UpdateConsumption(x, saleTotal));
        }
    }
}