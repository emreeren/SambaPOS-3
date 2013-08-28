using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Common
{
    public class AutomationCommandData
    {
        public AutomationCommand AutomationCommand { get; set; }
        public bool DisplayOnTicket { get; set; }
        public bool DisplayOnPayment { get; set; }
        public bool DisplayOnOrders { get; set; }
        public bool DisplayOnTicketList { get; set; }
        public bool DisplayUnderTicket { get; set; }
        public bool DisplayUnderTicket2 { get; set; }
        public bool DisplayOnCommandSelector { get; set; }
        public int VisualBehaviour { get; set; }
        public string EnabledStates { get; set; }
        public string VisibleStates { get; set; }

        public bool CanExecute(Ticket selectedTicket)
        {
            if (string.IsNullOrEmpty(EnabledStates)) return true;
            if (EnabledStates.Contains("IsClosed") && selectedTicket.IsClosed) return true;
            if (selectedTicket.IsClosed) return false;
            if (EnabledStates == "*") return true;
            if (DisplayOnOrders) return selectedTicket.Orders.Where(x => x.IsSelected).All(x => IsInState(x, EnabledStates));
            return IsInState(selectedTicket, EnabledStates);
        }

        public bool CanDisplay(Ticket selectedTicket)
        {
            if (string.IsNullOrEmpty(VisibleStates)) return true;
            if (VisibleStates == "*") return true;
            if (DisplayOnOrders) return selectedTicket.Orders.Where(x => x.IsSelected).All(x => IsInState(x, VisibleStates));
            return IsInState(selectedTicket, VisibleStates);
        }

        private bool IsInState(Ticket ticket, string states)
        {
            var result = states.Split(',').Where(x => x.Contains("=")).Select(x => x.Split('=')).Any(x => ticket.IsInState(x[0], x[1]));
            if (!result) result = states.Split(',').Where(x => !x.Contains("=")).Any(x => ticket.IsInState("*", x));
            return result;
        }

        private bool IsInState(Order order, string states)
        {
            var result = states.Split(',').Where(x => x.Contains("=")).Select(x => x.Split('=')).Any(x => order.IsInState(x[0], x[1]));
            if (!result) result = states.Split(',').Where(x => !x.Contains("=")).Any(x => order.IsInState("*", x));
            return result;
        }
    }
}