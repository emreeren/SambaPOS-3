using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class Department : Entity
    {
        public byte[] LastUpdateTime { get; set; }
        public string UserString { get { return Name; } }
        public int ScreenMenuId { get; set; }
        public bool IsFastFood { get; set; }
        public bool IsAlaCarte { get; set; }
        public bool IsTakeAway { get; set; }
        [StringLength(10)]
        public string PriceTag { get; set; }
        public virtual TicketTemplate TicketTemplate { get; set; }

        public int OpenTicketViewColumnCount { get; set; }

        private IList<ResourceScreen> _locationScreens;
        public virtual IList<ResourceScreen> LocationScreens
        {
            get { return _locationScreens; }
            set { _locationScreens = value; }
        }

        private static Department _all;
        public static Department All { get { return _all ?? (_all = new Department { Name = "*" }); } }

        public Department()
        {
            OpenTicketViewColumnCount = 5;
            _locationScreens = new List<ResourceScreen>();
        }
    }
}
