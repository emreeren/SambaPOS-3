using System.Collections.Generic;
using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.ViewModels
{
    public class OrderStateData
    {
        public OrderStateGroup OrderStateGroup { get; set; }
        public OrderState SelectedOrderState { get; set; }
        public Ticket Ticket { get; set; }
        public IEnumerable<Order> SelectedOrders { get; set; }
    }
}