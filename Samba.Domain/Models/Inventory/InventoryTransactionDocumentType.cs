using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class InventoryTransactionDocumentType : Entity, IOrderable
    {
        public virtual AccountTransactionType AccountTransactionType { get; set; }
        public virtual InventoryTransactionType InventoryTransactionType { get; set; }
        public int SourceResourceTypeId { get; set; }
        public int TargetResourceTypeId { get; set; }
        public int DefaultSourceResourceId { get; set; }
        public int DefaultTargetResourceId { get; set; }
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
    }
}
