using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionTemplate : IEntity, IOrderable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public int SourceAccountTemplateId { get; set; }
        public int TargetAccountTemplateId { get; set; }
        public int DefaultSourceAccountId { get; set; }
        public int DefaultTargetAccountId { get; set; }
    }
}
