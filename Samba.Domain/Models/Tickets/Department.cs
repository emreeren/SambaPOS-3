using System.ComponentModel.DataAnnotations;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class Department : EntityClass, IOrderable
    {
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }

        [StringLength(10)]
        public string PriceTag { get; set; }

        public int WarehouseId { get; set; }
        public int TicketTypeId { get; set; }
        public int ScreenMenuId { get; set; }
        public int TicketCreationMethod { get; set; }
        
        private static Department _all;
        public static Department All { get { return _all ?? (_all = new Department { Name = "*" }); } }

        private static Department _default;
        public static Department Default { get { return _default ?? (_default = new Department()); } }
    }
}
