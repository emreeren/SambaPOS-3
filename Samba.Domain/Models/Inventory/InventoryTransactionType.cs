using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionType : Entity
    {
        public int SourceWarehouseTypeId { get; set; }
        public int TargetWarehouseTypeId { get; set; }
        public int DefaultSourceWarehouseId { get; set; }
        public int DefaultTargetWarehouseId { get; set; }
    }
}