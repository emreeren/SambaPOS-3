using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class TaxTemplate : Entity
    {
        public decimal Rate { get; set; }
        public bool TaxIncluded { get; set; }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
    }
}
