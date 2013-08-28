using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Automation
{
    public class AutomationCommandMap : AbstractMap
    {
        public int AutomationCommandId { get; set; }
        public bool DisplayOnTicket { get; set; }
        public bool DisplayOnPayment { get; set; }
        public bool DisplayOnOrders { get; set; }
        public bool DisplayOnTicketList { get; set; }
        public bool DisplayUnderTicket { get; set; }
        public bool DisplayUnderTicket2 { get; set; }
        public bool DisplayOnCommandSelector { get; set; }

        public string EnabledStates { get; set; }
        public string VisibleStates { get; set; }

        public override void Initialize()
        {
            DisplayOnTicket = true;
            EnabledStates = "*";
            VisibleStates = "*";
            base.Initialize();
        }
    }
}
