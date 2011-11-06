using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class PrinterMap : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PrintJobId { get; set; }
        public byte[] LastUpdateTime { get; set; }
        public virtual Department Department { get; set; }
        public string MenuItemGroupCode { get; set; }
        public string TicketTag { get; set; }
        public virtual MenuItem MenuItem { get; set; }
        public virtual Printer Printer { get; set; }
        public virtual PrinterTemplate PrinterTemplate { get; set; }
    }
}
