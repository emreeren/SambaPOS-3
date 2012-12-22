using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class Warehouse : Entity,IOrderable
    {
        public int AccountId { get; set; }
        public int WarehouseTypeId { get; set; }
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
    }
}