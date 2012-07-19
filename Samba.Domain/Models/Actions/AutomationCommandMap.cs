using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class AutomationCommandMap : Value, IAbstractMapModel
    {
        public int AutomationCommandId { get; set; }
        public int TerminalId { get; set; }
        public int DepartmentId { get; set; }
        public int UserRoleId { get; set; }
        public bool DisplayOnTicket { get; set; }
        public bool DisplayOnPayment { get; set; }
        public int VisualBehaviour { get; set; } // 0 = Normal, 1 = Disable when ticket locked, 2 = Show when ticket locked
    }
}
