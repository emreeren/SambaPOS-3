using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    internal static class Helper
    {
        public static IList<Order> GetOrders(ActionData x, Ticket ticket)
        {
            IList<Order> orders = new List<Order>();
            var selectedOrder = x.GetDataValue<Order>("Order");

            if (selectedOrder != null && ticket != null && selectedOrder.SelectedQuantity > 0 &&
                selectedOrder.SelectedQuantity != selectedOrder.Quantity)
            {
                selectedOrder = ticket.ExtractSelectedOrder(selectedOrder);
                x.DataObject.Order = selectedOrder;
            }

            if (selectedOrder == null)
            {
                if (ticket != null)
                {
                    orders = ticket.Orders.Any(y => y.IsSelected)
                                 ? ticket.ExtractSelectedOrders().ToList()
                                 : ticket.Orders;
                    x.DataObject.Order = null;
                }
            }
            else orders.Add(selectedOrder);
            return orders;
        }
    }
}
