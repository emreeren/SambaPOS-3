using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class MenuItemTimer : Entity
    {
        public int PriceType { get; set; }
        public decimal PriceDuration { get; set; }
        public decimal MinTime { get; set; }
        public decimal TimeRounding { get; set; }

        private IList<MenuItemTimerMap> _menuItemTimerMaps;
        public virtual IList<MenuItemTimerMap> MenuItemTimerMaps
        {
            get { return _menuItemTimerMaps ?? (_menuItemTimerMaps = new List<MenuItemTimerMap>()); }
            set { _menuItemTimerMaps = value; }
        }

    }
}
