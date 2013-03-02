using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketType : EntityClass
    {
        public TicketType()
        {
            _entityTypeAssignments = new List<EntityTypeAssignment>();
        }

        public int ScreenMenuId { get; set; }
        public virtual Numerator TicketNumerator { get; set; }
        public virtual Numerator OrderNumerator { get; set; }
        public virtual AccountTransactionType SaleTransactionType { get; set; }
        public bool TaxIncluded { get; set; }

        private readonly IList<EntityTypeAssignment> _entityTypeAssignments;
        public virtual IList<EntityTypeAssignment> EntityTypeAssignments
        {
            get { return _entityTypeAssignments; }
        }

        private static TicketType _default;
        public static TicketType Default { get { return _default ?? (_default = new TicketType { SaleTransactionType = AccountTransactionType.Default }); } }
    }
}
