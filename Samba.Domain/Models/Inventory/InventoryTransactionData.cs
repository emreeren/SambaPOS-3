namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionData
    {
        public InventoryTransactionItem InventoryTransactionItem { get; set; }
        public int SourceWarehouseId { get; set; }
        public int TargetWarehouseId { get; set; }
    }
}