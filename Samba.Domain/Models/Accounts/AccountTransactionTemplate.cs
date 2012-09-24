using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionTemplate : Entity, IOrderable
    {
        private static AccountTransactionTemplate _default;
        public static AccountTransactionTemplate Default
        {
            get { return _default ?? (_default = new AccountTransactionTemplate()); }
        }

        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public int SourceAccountTemplateId { get; set; }
        public int TargetAccountTemplateId { get; set; }
        public int DefaultSourceAccountId { get; set; }
        public int DefaultTargetAccountId { get; set; }
    }
}
