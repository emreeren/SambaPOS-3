using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class InventoryTransaction : Entity
    {
        public DateTime Date { get; set; }

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
    }
}
