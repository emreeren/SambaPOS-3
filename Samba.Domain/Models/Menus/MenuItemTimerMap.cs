using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class MenuItemTimerMap : AbstractMap
    {
        public int MenuItemTimerId { get; set; }
        public string MenuItemGroupCode { get; set; }
        public int MenuItemId { get; set; }
    }
}
