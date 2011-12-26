using System.Collections.Generic;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IInventoryService : IService
    {
        PeriodicConsumption GetPreviousPeriodicConsumption(IWorkspace workspace);
        PeriodicConsumption GetCurrentPeriodicConsumption(IWorkspace workspace);
        void CalculateCost(PeriodicConsumption pc, WorkPeriod workPeriod);
        IEnumerable<string> GetInventoryItemNames();
        int GetPeriodicConsumptionItemCountByInventoryItem(int id);
        IEnumerable<string> GetGroupCodes();
        decimal RecipeCountByPortion(Recipe model);
    }
}
