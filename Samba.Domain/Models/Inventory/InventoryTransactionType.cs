using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionType : Entity, IOrderable
    {
        public int SourceResourceTypeId { get; set; }
        public int TargetResourceTypeId { get; set; }
        public int DefaultSourceResourceId { get; set; }
        public int DefaultTargetResourceId { get; set; }
        public virtual AccountTransactionType AccountTransactionType { get; set; }
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
    }
}