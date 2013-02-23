using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class Account : EntityClass
    {
        public int AccountTypeId { get; set; }
        public int ForeignCurrencyId { get; set; }
        private static Account _null;
        public static Account Null { get { return _null ?? (_null = new Account { Name = "*" }); } }

        public static Account Create(int accountTypeId)
        {
            return new Account { AccountTypeId = accountTypeId };
        }
    }
}
