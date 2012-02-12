using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionTemplate : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SourceAccountTemplateId { get; set; }
        public int TargetAccountTemplateId { get; set; }
        public int DefaultSourceAccountId { get; set; }
        public int DefaultTargetAccountId { get; set; }
    }
}
