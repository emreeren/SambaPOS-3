using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransaction : ValueClass
    {
        public int InventoryTransactionDocumentId { get; set; }
        public int InventoryTransactionTypeId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int TargetWarehouseId { get; set; }
        public DateTime Date { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }
        public string Unit { get; set; }
        public int Multiplier { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
