using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTag : IOrderable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public decimal Price { get; set; }
        public int MenuItemId { get; set; }
    }
}
