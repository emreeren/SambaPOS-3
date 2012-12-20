using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class PeriodicConsumption : Entity, ICacheable
    {
        public int WorkPeriodId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime LastUpdateTime { get; set; }

        private readonly IList<PeriodicConsumptionItem> _periodicConsumptionItems;
        public virtual IList<PeriodicConsumptionItem> PeriodicConsumptionItems
        {
            get { return _periodicConsumptionItems; }
        }

        private readonly IList<CostItem> _costItems;
        public virtual IList<CostItem> CostItems
        {
            get { return _costItems; }
        }

        public PeriodicConsumption()
        {
            _periodicConsumptionItems = new List<PeriodicConsumptionItem>();
            _costItems = new List<CostItem>();
            LastUpdateTime = DateTime.Now;
        }

        private decimal GetCost(RecipeItem recipeItem)
        {
            var pci = PeriodicConsumptionItems.Single(x => x.InventoryItemId == recipeItem.InventoryItem.Id);
            if (pci.GetPredictedConsumption() > 0)
            {
                var cost = recipeItem.Quantity * (pci.Cost / pci.UnitMultiplier);
                cost = (pci.GetConsumption() * cost) / pci.GetPredictedConsumption();
                return cost;
            }
            return 0;
        }

        private void UpdateCost(Recipe recipe)
        {
            if (recipe == null) return;
            var ci = CostItems.SingleOrDefault(x => x.PortionId == recipe.Portion.Id);
            if (ci == null) return;
            var totalcost = recipe.FixedCost + recipe.GetValidRecipeItems().Sum(recipeItem => GetCost(recipeItem));
            ci.Cost = decimal.Round(totalcost, 2);
        }

        public void UpdateCost(IEnumerable<Recipe> recipes)
        {
            recipes.ToList().ForEach(UpdateCost);
        }

        private decimal UpdateConsumption(RecipeItem recipeItem, decimal saleTotal)
        {
            var pci = PeriodicConsumptionItems.Single(x => x.InventoryItemId == recipeItem.InventoryItem.Id);
            pci.Consumption += (recipeItem.Quantity * saleTotal) / pci.UnitMultiplier;
            return recipeItem.Quantity * (pci.Cost / pci.UnitMultiplier);
        }

        public void UpdateConsumption(Recipe recipe, string menuItemName, decimal saleTotal)
        {
            if (recipe == null) return;
            var totalCost = recipe.GetValidRecipeItems().Sum(recipeItem => UpdateConsumption(recipeItem, saleTotal));
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

        public void CreatePeriodicConsumptionItem(InventoryItem inventoryItem, PeriodicConsumption previousPc, IEnumerable<InventoryTransactionItem> transactionItems)
        {
            var pci = PeriodicConsumptionItem.Create(inventoryItem);
            PeriodicConsumptionItems.Add(pci);
            var previousCost = 0m;
            if (previousPc != null)
            {
                var previousPci = previousPc.PeriodicConsumptionItems.SingleOrDefault(x => x.InventoryItemId == inventoryItem.Id);
                if (previousPci != null)
                    pci.InStock =
                        previousPci.PhysicalInventory != null
                            ? previousPci.PhysicalInventory.GetValueOrDefault(0)
                            : previousPci.GetInventoryPrediction();
                if (previousPci != null)
                    previousCost = previousPci.Cost * pci.InStock;
            }
            var tim = transactionItems.Where(x => x.InventoryItem.Id == inventoryItem.Id).ToList();
            pci.Purchase = tim.Sum(x => x.Quantity * x.Multiplier) / pci.UnitMultiplier;
            var totalPrice = tim.Sum(x => x.Price * x.Quantity);
            if (pci.InStock > 0 || pci.Purchase > 0)
                pci.Cost = decimal.Round((totalPrice + previousCost) / (pci.InStock + pci.Purchase), 2);
        }

        public static PeriodicConsumption Create(WorkPeriod currentWorkPeriod)
        {
            return new PeriodicConsumption
            {
                WorkPeriodId = currentWorkPeriod.Id,
                Name = currentWorkPeriod.StartDate + " - " +
                       currentWorkPeriod.EndDate,
                StartDate = currentWorkPeriod.StartDate,
                EndDate = currentWorkPeriod.EndDate
            };
        }

        public void CreatePeriodicConsumptionItems(IEnumerable<InventoryItem> inventoryItems, PeriodicConsumption previousPc, List<InventoryTransactionItem> transactionItems)
        {
            foreach (var inventoryItem in inventoryItems)
            {
                CreatePeriodicConsumptionItem(inventoryItem, previousPc, transactionItems);
            }
        }

    }
}
