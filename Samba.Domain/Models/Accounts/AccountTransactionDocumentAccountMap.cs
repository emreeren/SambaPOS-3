using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentAccountMap : Value
    {
        public int AccountTransactionDocumentTemplateId { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public int MappedAccountId { get; set; }
        public string MappedAccountName { get; set; }
    }
}
