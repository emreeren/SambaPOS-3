using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryItem : EntityClass
    {
        public string GroupCode { get; set; }

        public string BaseUnit { get; set; }

        public string TransactionUnit { get; set; }

        public int TransactionUnitMultiplier { get; set; }

        public decimal Multiplier
        {
            get
            {
                return TransactionUnitMultiplier > 0
                           ? TransactionUnitMultiplier
                           : 1;
            }
        }
    }
}
