using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class MenuAssignment : ValueClass, IOrderable
    {
        public int TicketTypeId { get; set; }
        public int TerminalId { get; set; }
        public int MenuId { get; set; }
        public string TerminalName { get; set; }
        public int SortOrder { get; set; }
        public string Name { get { return TerminalName; } }
        public string UserString { get { return Name; } }

    }
}