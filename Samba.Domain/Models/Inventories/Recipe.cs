using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class Recipe : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual MenuItemPortion Portion { get; set; }
        public virtual IList<RecipeItem> RecipeItems { get; set; }
        public decimal FixedCost { get; set; }

        public Recipe()
        {
            RecipeItems = new List<RecipeItem>();
        }
    }
}
