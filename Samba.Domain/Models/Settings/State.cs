using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class State : EntityClass
    {
        public string GroupName { get; set; }
        public int StateType { get; set; } // 0 = Entity State, 1 = Ticket State, 2 = Order State
        public string Color { get; set; }
        public bool ShowOnEndOfDayReport { get; set; }
        public bool ShowOnProductReport { get; set; }
        public bool ShowOnTicket { get; set; }

        private static State _default;
        public static State Default
        {
            get { return _default ?? (_default = new State { Color = "Gainsboro" }); }
        }
    }
}
