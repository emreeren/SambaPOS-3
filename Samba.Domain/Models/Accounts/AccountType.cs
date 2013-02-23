using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountType : EntityClass, IOrderable
    {
        public int DefaultFilterType { get; set; }
        public int WorkingRule { get; set; }  //0 -Permit Credit & Debit, 1- Disallow Debit, 2- Disallow Credit
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
        public string Tags { get; set; }
    }
}
