using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketType : Entity
    {
        public TicketType()
        {
            _resourceTypeAssignments = new List<ResourceTypeAssignment>();
        }

        public int ScreenMenuId { get; set; }
        public virtual Numerator TicketNumerator { get; set; }
        public virtual Numerator OrderNumerator { get; set; }
        public virtual AccountTransactionType SaleTransactionType { get; set; }
        public bool TaxIncluded { get; set; }

        private readonly IList<ResourceTypeAssignment> _resourceTypeAssignments;
        public virtual IList<ResourceTypeAssignment> ResourceTypeAssignments
        {
            get { return _resourceTypeAssignments; }
        }

        private static TicketType _default;
        public static TicketType Default { get { return _default ?? (_default = new TicketType { SaleTransactionType = AccountTransactionType.Default }); } }
    }
}
