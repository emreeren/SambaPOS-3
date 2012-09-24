using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTemplate : Entity
    {
        public virtual Numerator TicketNumerator { get; set; }
        public virtual Numerator OrderNumerator { get; set; }
        public virtual AccountTransactionTemplate SaleTransactionTemplate { get; set; }

        private static TicketTemplate _default;
        public static TicketTemplate Default { get { return _default ?? (_default = new TicketTemplate { SaleTransactionTemplate = AccountTransactionTemplate.Default }); } }
    }
}
