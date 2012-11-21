using Samba.Domain.Models.Automation;

namespace Samba.Services.Common
{
    public class AutomationCommandData
    {
        public AutomationCommand AutomationCommand { get; set; }
        public bool DisplayOnTicket { get; set; }
        public bool DisplayOnPayment { get; set; }
        public bool DisplayOnOrders { get; set; }
        public int VisualBehaviour { get; set; }
    }
}