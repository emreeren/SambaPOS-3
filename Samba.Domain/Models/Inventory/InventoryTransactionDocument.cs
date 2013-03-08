using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionDocument : EntityClass
    {
        public DateTime Date { get; set; }

        private IList<InventoryTransaction> _transactionItems;
        public virtual IList<InventoryTransaction> TransactionItems
        {
            get { return _transactionItems; }
            set { _transactionItems = value; }
        }

        public InventoryTransactionDocument()
        {
            _transactionItems = new List<InventoryTransaction>();
            Date = DateTime.Now;
        }

        public InventoryTransaction Add(InventoryTransactionType transactionType, InventoryItem inventoryItem, decimal price, decimal quantity, string unit, int multiplier)
        {
            var result = new InventoryTransaction
                             {
                                 Date = DateTime.Now,
                                 InventoryTransactionTypeId = transactionType.Id,
                                 SourceWarehouseId = transactionType.DefaultSourceWarehouseId,
                                 TargetWarehouseId = transactionType.DefaultTargetWarehouseId,
                                 InventoryItem = inventoryItem,
                                 Multiplier = multiplier,
                                 Price = price,
                                 Quantity = quantity,
                                 Unit = unit
                             };

            TransactionItems.Add(result);
            return result;
        }

        public decimal GetSum()
        {
            return _transactionItems.Sum(x => x.Price * x.Quantity);
        }
    }
}
