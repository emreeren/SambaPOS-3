using System.Collections.Generic;
using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.ViewModels
{
    public class OrderTagData
    {
        public OrderTagGroup OrderTagGroup { get; set; }
        public OrderTag SelectedOrderTag { get; set; }
        public Ticket Ticket { get; set; }
        public IEnumerable<Order> SelectedOrders { get; set; }
    }
}
