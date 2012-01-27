using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionTemplate:IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual AccountTemplate SourceAccountTemplate { get; set; }
        public virtual AccountTemplate TargetAccountTemplate { get; set; }
        public virtual AccountTemplate TransactionAccountTemplate { get; set; }
    }
}
