using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Automation
{
    public class AutomationCommandMap : AbstractMap
    {
        public int AutomationCommandId { get; set; }
        public bool DisplayOnTicket { get; set; }
        public bool DisplayOnPayment { get; set; }
        public bool DisplayOnOrders { get; set; }
        public string EnabledStates { get; set; }
        public string VisibleStates { get; set; }
    }
}
