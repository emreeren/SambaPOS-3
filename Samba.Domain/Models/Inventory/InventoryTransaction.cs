using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransaction : Entity
    {
        public DateTime Date { get; set; }
        public int InventoryTransactionTypeId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int TargetWarehouseId { get; set; }

        private readonly IList<InventoryTransactionItem> _transactionItems;
        public virtual IList<InventoryTransactionItem> TransactionItems
        {
            get { return _transactionItems; }
        }

        public InventoryTransaction()
        {
            _transactionItems = new List<InventoryTransactionItem>();
            Date = DateTime.Now;
        }

        public static InventoryTransaction Create(InventoryTransactionType transactionType)
        {
            return new InventoryTransaction
                       {
                           InventoryTransactionTypeId = transactionType.Id,
                           SourceWarehouseId = transactionType.DefaultSourceWarehouseId,
                           TargetWarehouseId = transactionType.DefaultTargetWarehouseId
                       };
        }


        public void Add(InventoryItem inventoryItem, decimal price, decimal quantity, string unit, int multiplier)
        {
            var result = new InventoryTransactionItem
                             {
                                 InventoryItem = inventoryItem,
                                 Multiplier = multiplier,
                                 Price = price,
                                 Quantity = quantity,
                                 Unit = unit
                             };

            TransactionItems.Add(result);
        }

        public void SetSourceWarehouse(Warehouse warehouse)
        {
            SourceWarehouseId = warehouse.Id;
        }

        public void SetTargetWarehouse(Warehouse warehouse)
        {
            TargetWarehouseId = warehouse.Id;
        }
    }
}
