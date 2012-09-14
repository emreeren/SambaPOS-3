using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class Account : Entity
    {
        public int AccountTemplateId { get; set; }
        public int ForeignCurrencyId { get; set; }
        private static Account _null;
        public static Account Null { get { return _null ?? (_null = new Account { Name = "*" }); } }
    }
}
