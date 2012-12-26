using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransaction : Entity
    {
        public DateTime Date { get; set; }
        public int InventoryTransactionTypeId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int TargetWarehouseId { get; set; }
        public virtual AccountTransactionType AccountTransactionType { get; set; }
        public int SourceAccountId { get; set; }
        public int TargetAccountId { get; set; }
        public virtual AccountTransactionDocument TransactionDocument { get; set; }

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
                           TargetWarehouseId = transactionType.DefaultTargetWarehouseId,
                           AccountTransactionType = transactionType.AccountTransactionType
                       };
        }

        public InventoryTransactionItem Add(InventoryItem inventoryItem, decimal price, decimal quantity, string unit, int multiplier)
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
            return result;
        }

        public void SetSourceWarehouse(Warehouse warehouse)
        {
            SourceWarehouseId = warehouse.Id;
            SourceAccountId = warehouse.AccountId;
        }

        public void SetTargetWarehouse(Warehouse warehouse)
        {
            TargetWarehouseId = warehouse.Id;
            TargetAccountId = warehouse.AccountId;
        }

        public decimal GetSum()
        {
            return _transactionItems.Sum(x => x.Price * x.Quantity);
        }

        public void Recalculate()
        {
            if (AccountTransactionType == null) return;
            if (SourceAccountId == 0 || TargetAccountId == 0) return;

            if (TransactionDocument == null) TransactionDocument = new AccountTransactionDocument();
            var transaction = TransactionDocument.AccountTransactions.SingleOrDefault(x => x.AccountTransactionTypeId == AccountTransactionType.Id);
            if (transaction == null)
            {
                transaction = TransactionDocument.AddNewTransaction(AccountTransactionType,
                                                                    AccountTransactionType.SourceAccountTypeId,
                                                                    SourceAccountId);
            }
            transaction.UpdateAccounts(AccountTransactionType.SourceAccountTypeId, SourceAccountId);
            transaction.UpdateAccounts(AccountTransactionType.TargetAccountTypeId, TargetAccountId);
            transaction.UpdateAmount(GetSum(), 1);
        }
    }
}
