using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTemplate : Entity
    {
        public int ScreenMenuId { get; set; }
        public virtual Numerator TicketNumerator { get; set; }
        public virtual Numerator OrderNumerator { get; set; }
        public virtual AccountTransactionType SaleTransactionType { get; set; }

        private static TicketTemplate _default;
        public static TicketTemplate Default { get { return _default ?? (_default = new TicketTemplate { SaleTransactionType = AccountTransactionType.Default }); } }
    }
}
