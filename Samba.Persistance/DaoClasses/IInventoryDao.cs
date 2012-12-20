using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Tickets;

namespace Samba.Persistance.DaoClasses
{
    public interface IInventoryDao
    {
        IEnumerable<InventoryTransactionItem> GetTransactionItems(DateTime workperiodStartDate);
        IEnumerable<Order> GetOrdersFromRecipes(DateTime startDate);
        Recipe GetRecipe(string portionName, int menuItemId);
        IEnumerable<string> GetGroupCodes();
        IEnumerable<string> GetInventoryItemNames();
        bool RecipeExists();
        IEnumerable<InventoryItem> GetInventoryItems();
        PeriodicConsumption GetPeriodicConsumptionByWorkPeriodId(int workPeriodId);
    }
}
