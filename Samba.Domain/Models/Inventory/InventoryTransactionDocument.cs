using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionDocument : Entity
    {
        public DateTime Date { get; set; }
        public int InventoryTransactionDocumentTypeId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int TargetWarehouseId { get; set; }
        public virtual AccountTransactionType AccountTransactionType { get; set; }
        public int SourceAccountId { get; set; }
        public int TargetAccountId { get; set; }
        public int SourceResourceId { get; set; }
        public int TargetResourceId { get; set; }
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
                           SourceResourceId = transactionType.DefaultSourceResourceId,
                           TargetResourceId = transactionType.DefaultTargetResourceId,
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

        public void SetSourceResource(Resource resource)
        {
            SourceResourceId = resource.Id;
            SourceWarehouseId = resource.WarehouseId;
            SourceAccountId = resource.AccountId;
        }

        public void SetTargetResource(Resource resource)
        {
            TargetResourceId = resource.Id;
            TargetWarehouseId = resource.WarehouseId;
            TargetAccountId = resource.AccountId;
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
