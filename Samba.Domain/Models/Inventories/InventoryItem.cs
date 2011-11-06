using System.ComponentModel.DataAnnotations;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class InventoryItem : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupCode { get; set; }
        [StringLength(10)]
        public string BaseUnit { get; set; }
        [StringLength(10)]
        public string TransactionUnit { get; set; }
        public int TransactionUnitMultiplier { get; set; }
    }
}

