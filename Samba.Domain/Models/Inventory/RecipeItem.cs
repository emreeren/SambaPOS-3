using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class RecipeItem : Value
    {
        public int RecipeId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }
        public decimal Quantity { get; set; }
    }
}
