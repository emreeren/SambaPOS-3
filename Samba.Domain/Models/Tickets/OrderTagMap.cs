using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTagMap : Value, IAbstractMapModel
    {
        public int OrderTagGroupId { get; set; }
        public int DepartmentId { get; set; }
        public int UserRoleId { get; set; }
        public string MenuItemGroupCode { get; set; }
        public int MenuItemId { get; set; }
    }
}
