using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class PeriodicConsumptionItem : Value
    {
        public int PeriodicConsumptionId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }
        public decimal UnitMultiplier { get; set; }
        public decimal InStock { get; set; }
        public decimal Purchase { get; set; }
        public decimal Consumption { get; set; }
        public decimal? PhysicalInventory { get; set; }
        public decimal Cost { get; set; }

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
            return Consumption;
        }

        public decimal GetConsumption()
        {
            if (PhysicalInventory == null) return GetPredictedConsumption();
            return (InStock + Purchase) - PhysicalInventory.GetValueOrDefault(0);
        }
    }
}
