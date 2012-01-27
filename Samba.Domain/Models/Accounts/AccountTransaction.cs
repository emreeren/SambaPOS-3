using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransaction : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }
        public decimal Liability { get; set; }
        public decimal Receivable { get; set; }
    }
}
