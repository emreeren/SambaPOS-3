using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class InventoryTransaction : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }

        private IList<InventoryTransactionItem> _transactionItems;
        public virtual IList<InventoryTransactionItem> TransactionItems
        {
            get { return _transactionItems; }
            set { _transactionItems = value; }
        }

        public InventoryTransaction()
        {
            _transactionItems = new List<InventoryTransactionItem>();
            Date = DateTime.Now;
        }
    }
}
