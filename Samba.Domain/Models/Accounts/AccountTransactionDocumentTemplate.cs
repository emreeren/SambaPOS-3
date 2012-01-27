using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentTemplate : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
        public virtual Account SourceAccount { get; set; }
        public virtual Account TargetAccount { get; set; }
        public virtual Account TransactionAccount { get; set; }
    }
}
