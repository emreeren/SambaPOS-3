using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.DaoClasses.Implementations
{
    [Export(typeof(IInventoryDao))]
    class InventoryDao : IInventoryDao
    {
        [ImportingConstructor]
        public InventoryDao()
        {
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<InventoryItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.InventoryItem)));
            ValidatorRegistry.RegisterDeleteValidator<InventoryItem>(x => Dao.Exists<PeriodicConsumptionItem>(y => y.InventoryItem.Id == x.Id), Resources.InventoryItem, Resources.EndOfDayRecord);
            ValidatorRegistry.RegisterSaveValidator(new RecipeSaveValidator());
        }

        public IEnumerable<InventoryTransactionItem> GetTransactionItems(DateTime workPeriodStartDate)
        {
            return Dao.Query<InventoryTransaction>(x =>
                                       x.Date > workPeriodStartDate,
                                       x => x.TransactionItems,
                                       x => x.TransactionItems.Select(y => y.InventoryItem)).SelectMany(x => x.TransactionItems);
        }

        public IEnumerable<Order> GetOrdersFromRecipes(DateTime startDate)
        {
            var recipeItemIds = Dao.Select<Recipe, int>(x => x.Portion.MenuItemId, x => x.Portion != null).Distinct();
            var tickets = Dao.Query<Ticket>(x => x.Date > startDate,
                                            x => x.Orders,
                                            x => x.Orders.Select(y => y.OrderTagValues));
            return tickets.SelectMany(x => x.Orders)
                    .Where(x => (x.DecreaseInventory || x.IncreaseInventory) && recipeItemIds.Contains(x.MenuItemId));
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
                const string mitemName = "[Menu Item Name]"; // todo:fix;
                return string.Format(Resources.ThereIsAnotherRecipeFor_f, mitemName);
            }
            return "";
        }
    }
}
