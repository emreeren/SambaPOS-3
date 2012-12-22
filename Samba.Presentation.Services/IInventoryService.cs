using System.Collections.Generic;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Settings;

namespace Samba.Presentation.Services
{
    public interface IInventoryService
    {
        PeriodicConsumption GetPreviousPeriodicConsumption(int warehouseId);
        PeriodicConsumption GetCurrentPeriodicConsumption(int warehouseId);
        void CalculateCost(PeriodicConsumption pc, WorkPeriod workPeriod);
        IEnumerable<string> GetInventoryItemNames();
        IEnumerable<string> GetGroupCodes();
        void SavePeriodicConsumption(PeriodicConsumption pc);
        decimal GetInventory(InventoryItem inventoryItem, Warehouse warehouse);
        IEnumerable<PeriodicConsumption> GetCurrentPeriodicConsumptions();
        void DoWorkPeriodStart();
        void DoWorkPeriodEnd();
    }
}
