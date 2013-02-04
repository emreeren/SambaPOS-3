using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionType : EntityClass, IOrderable
    {
        public int SourceWarehouseTypeId { get; set; }
        public int TargetWarehouseTypeId { get; set; }
        public int DefaultSourceWarehouseId { get; set; }
        public int DefaultTargetWarehouseId { get; set; }
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
    }
}