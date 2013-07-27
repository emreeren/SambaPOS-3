using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketType : EntityClass, IOrderable
    {
        public TicketType()
        {
            _entityTypeAssignments = new List<EntityTypeAssignment>();
            _menuAssignments = new List<MenuAssignment>();
        }

        public int ScreenMenuId { get; set; }
        public virtual Numerator TicketNumerator { get; set; }
        public virtual Numerator OrderNumerator { get; set; }
        public virtual AccountTransactionType SaleTransactionType { get; set; }
        public bool TaxIncluded { get; set; }
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }

        private IList<EntityTypeAssignment> _entityTypeAssignments;
        public virtual IList<EntityTypeAssignment> EntityTypeAssignments
        {
            get { return _entityTypeAssignments; }
            set { _entityTypeAssignments = value; }
        }

        private IList<MenuAssignment> _menuAssignments;
        public virtual IList<MenuAssignment> MenuAssignments
        {
            get { return _menuAssignments; }
            set { _menuAssignments = value; }
        }

        private static TicketType _default;
        public static TicketType Default { get { return _default ?? (_default = new TicketType { SaleTransactionType = AccountTransactionType.Default }); } }

        public int GetScreenMenuId(Terminal terminal)
        {
            if (terminal == null) return ScreenMenuId;
            var result = MenuAssignments.FirstOrDefault(x => x.TerminalId == terminal.Id);
            return result != null ? result.MenuId : ScreenMenuId;
        }
    }
}
