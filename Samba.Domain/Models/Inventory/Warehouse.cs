using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class Warehouse : Entity
    {
        public int AccountId { get; set; }
        public int WarehouseTypeId { get; set; }
    }
}