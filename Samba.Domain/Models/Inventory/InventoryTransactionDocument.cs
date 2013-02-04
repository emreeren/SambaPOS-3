using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionDocument : EntityClass
    {
        public DateTime Date { get; set; }
        public int InventoryTransactionDocumentTypeId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int TargetWarehouseId { get; set; }
        public int SourceAccountId { get; set; }
        public int TargetAccountId { get; set; }
        public int SourceEntityId { get; set; }
        public int TargetEntityId { get; set; }
        public virtual AccountTransactionType AccountTransactionType { get; set; }
        public virtual AccountTransactionDocument TransactionDocument { get; set; }

        private readonly IList<InventoryTransaction> _transactionItems;
        public virtual IList<InventoryTransaction> TransactionItems
        {
            get { return _transactionItems; }
        }

        public InventoryTransactionDocument()
        {
            _transactionItems = new List<InventoryTransaction>();
            Date = DateTime.Now;
        }

        public static InventoryTransactionDocument Create(InventoryTransactionDocumentType transactionType)
        {
            return new InventoryTransactionDocument
                       {
                           InventoryTransactionDocumentTypeId = transactionType.Id,
                           SourceWarehouseId = transactionType.InventoryTransactionType.DefaultSourceWarehouseId,
                           TargetWarehouseId = transactionType.InventoryTransactionType.DefaultTargetWarehouseId,
                           SourceEntityId = transactionType.DefaultSourceEntityId,
                           TargetEntityId = transactionType.DefaultTargetEntityId,
                           AccountTransactionType = transactionType.AccountTransactionType
                       };
        }

        public InventoryTransaction Add(InventoryItem inventoryItem, decimal price, decimal quantity, string unit, int multiplier)
        {
            var result = new InventoryTransaction
                             {
                                 Date = DateTime.Now,
                                 SourceWarehouseId = SourceWarehouseId,
                                 TargetWarehouseId = TargetWarehouseId,
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
        }

        public void SetTargetWarehouse(Warehouse warehouse)
        {
            TargetWarehouseId = warehouse.Id;
        }

        public void SetSourceEntity(Entity entity)
        {
            SourceEntityId = entity.Id;
            SourceWarehouseId = entity.WarehouseId;
            SourceAccountId = entity.AccountId;
        }

        public void SetTargetEntity(Entity entity)
        {
            TargetEntityId = entity.Id;
            TargetWarehouseId = entity.WarehouseId;
            TargetAccountId = entity.AccountId;
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

            var transaction =
                TransactionDocument.AccountTransactions.SingleOrDefault(x => x.AccountTransactionTypeId == AccountTransactionType.Id)
                ?? TransactionDocument.AddNewTransaction(AccountTransactionType, AccountTransactionType.SourceAccountTypeId, SourceAccountId);

            transaction.UpdateAccounts(AccountTransactionType.SourceAccountTypeId, SourceAccountId);
            transaction.UpdateAccounts(AccountTransactionType.TargetAccountTypeId, TargetAccountId);
            transaction.UpdateAmount(GetSum(), 1);
        }
    }
}
