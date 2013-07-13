using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    internal static  class Helper
    {
        public static IList<Order> GetOrders(ActionData x, Ticket ticket)
        {
            IList<Order> orders = new List<Order>();
            var selectedOrder = x.GetDataValue<Order>("Order");
            if (selectedOrder == null)
            {
                if (ticket != null)
                {
                    orders = ticket.Orders.Any(y => y.IsSelected)
                                 ? ticket.ExtractSelectedOrders().ToList()
                                 : ticket.Orders;
                }
            }
            else orders.Add(selectedOrder);
            return orders;
        }
    }
}
