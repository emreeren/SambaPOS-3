using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class PeriodicConsumptionItem : ValueClass
    {
        public int PeriodicConsumptionId { get; set; }
        public int WarehouseConsumptionId { get; set; }
        public int InventoryItemId { get; set; }
        public string InventoryItemName { get; set; }
        public string UnitName { get; set; }
        public decimal UnitMultiplier { get; set; }
        public decimal InStock { get; set; }
        public decimal Added { get; set; }
        public decimal Removed { get; set; }
        public decimal Consumption { get; set; }
        public decimal? PhysicalInventory { get; set; }
        public decimal Cost { get; set; }

        public decimal Purchase { get { return Added; } }

        public decimal GetInventoryPrediction()
        {
            return (InStock + Purchase) - GetPredictedConsumption();
        }

        public decimal GetPhysicalInventory()
        {
            return (InStock + Purchase) - GetConsumption();
        }

        public decimal GetPredictedConsumption()
        {
            return Consumption + Removed;
        }

        public decimal GetConsumption()
        {
            if (PhysicalInventory == null) return GetPredictedConsumption();
            return (InStock + Purchase) - PhysicalInventory.GetValueOrDefault(0);
        }

        public static PeriodicConsumptionItem Create(InventoryItem inventoryItem)
        {
            return new PeriodicConsumptionItem
                {
                    InventoryItemId = inventoryItem.Id,
                    InventoryItemName = inventoryItem.Name,
                    UnitName = inventoryItem.TransactionUnit ?? inventoryItem.BaseUnit,
                    UnitMultiplier = inventoryItem.TransactionUnitMultiplier > 0
                                         ? inventoryItem.TransactionUnitMultiplier
                                         : 1
                };
        }
    }
}
