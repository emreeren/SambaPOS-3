using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class Department : Entity
    {
        public string UserString { get { return Name; } }
        public int ScreenMenuId { get; set; }
        [StringLength(10)]
        public string PriceTag { get; set; }
        public int TicketCreationMethod { get; set; }
        public virtual TicketTemplate TicketTemplate { get; set; }

        private readonly IList<ResourceScreen> _resourceScreens;
        public virtual IList<ResourceScreen> ResourceScreens
        {
            get { return _resourceScreens; }
        }

        private static Department _all;
        public static Department All { get { return _all ?? (_all = new Department { Name = "*" }); } }

        private static Department _default;
        public static Department Default { get { return _default ?? (_default = new Department { TicketTemplate = TicketTemplate.Default }); } }

        public Department()
        {
            _resourceScreens = new List<ResourceScreen>();
        }
    }
}
