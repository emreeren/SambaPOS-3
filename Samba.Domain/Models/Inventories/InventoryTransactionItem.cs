using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class InventoryTransactionItem : Value
    {
        public int TransactionId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }
        public string Unit { get; set; }
        public int Multiplier { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
