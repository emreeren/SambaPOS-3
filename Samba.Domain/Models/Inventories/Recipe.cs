using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class Recipe : Entity
    {
        public virtual MenuItemPortion Portion { get; set; }
        public decimal FixedCost { get; set; }
        private readonly IList<RecipeItem> _recipeItems;
        public virtual IList<RecipeItem> RecipeItems
        {
            get { return _recipeItems; }
        }

        public Recipe()
        {
            _recipeItems = new List<RecipeItem>();
        }
    }
}
