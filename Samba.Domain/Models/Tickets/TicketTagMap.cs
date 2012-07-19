using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTagMap : Value, IAbstractMapModel
    {
        public int TicketTagGroupId { get; set; }
        public int TerminalId { get; set; }
        public int DepartmentId { get; set; }
        public int UserRoleId { get; set; }
    }
}
