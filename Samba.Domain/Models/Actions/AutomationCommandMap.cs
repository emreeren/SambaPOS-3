using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class AutomationCommandMap : AbstractMap
    {
        public int AutomationCommandId { get; set; }
        public bool DisplayOnTicket { get; set; }
        public bool DisplayOnPayment { get; set; }
        public int VisualBehaviour { get; set; } // 0 = Normal, 1 = Disable when ticket locked, 2 = Show when ticket locked,3=Disable when ticket active,4 = Display when ticket active
    }
}
