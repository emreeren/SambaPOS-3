using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class PeriodicConsumption : EntityClass, ICacheable
    {
        public int WorkPeriodId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime LastUpdateTime { get; set; }

        private IList<WarehouseConsumption> _warehouseConsumptions;
        public virtual IList<WarehouseConsumption> WarehouseConsumptions
        {
            get { return _warehouseConsumptions; }
            set { _warehouseConsumptions = value; }
        }

        public PeriodicConsumption()
        {
            _warehouseConsumptions = new List<WarehouseConsumption>();
            LastUpdateTime = DateTime.Now;
        }

        public void UpdateFinalCost(IList<Recipe> recipes, int warehouseId)
        {
            var whc = WarehouseConsumptions.Single(x => x.WarehouseId == warehouseId);
            whc.UpdateFinalCost(recipes);
        }

        public void UpdateConsumption(Recipe recipe, decimal saleTotal, int warehouseId)
        {
            var whc = WarehouseConsumptions.Single(x => x.WarehouseId == warehouseId);
            whc.UpdateConsumption(recipe, saleTotal);
        }

        public void CreateCostItem(Recipe recipe, string menuItemName, decimal saleTotal, int warehouseId)
        {
            var whc = WarehouseConsumptions.Single(x => x.WarehouseId == warehouseId);
            whc.CreateCostItem(recipe, menuItemName, saleTotal);
        }

        public static PeriodicConsumption Create(WorkPeriod currentWorkPeriod, IEnumerable<int> warehouseIds)
        {
            var result = new PeriodicConsumption
            {
                WorkPeriodId = currentWorkPeriod.Id,
                Name = currentWorkPeriod.StartDate + " - " +
                       currentWorkPeriod.EndDate,
                StartDate = currentWorkPeriod.StartDate,
                EndDate = currentWorkPeriod.EndDate
            };
            result.CreateWarehouseConsumptions(warehouseIds);
            return result;
        }

        private void CreateWarehouseConsumptions(IEnumerable<int> warehouseIds)
        {
            foreach (var warehouseId in warehouseIds)
            {
                var whc = new WarehouseConsumption() { WarehouseId = warehouseId };
                WarehouseConsumptions.Add(whc);
            }
        }

        public void CreatePeriodicConsumptionItems( int warehouseId, IList<InventoryItem> inventoryItems, PeriodicConsumption previousPc, List<InventoryTransaction> transactionItems)
        {
            var warehouseConsumption = WarehouseConsumptions.Single(x => x.WarehouseId == warehouseId);
            var previousWhc = previousPc != null
                                  ? previousPc.WarehouseConsumptions.SingleOrDefault(x => x.WarehouseId == warehouseConsumption.WarehouseId)
                                  : null;
            warehouseConsumption.CreatePeriodicConsumptionItems( inventoryItems, previousWhc, transactionItems);
        }
    }
}
