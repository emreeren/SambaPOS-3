using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.BasicReports.Reports
{
    internal static class MenuGroupBuilder
    {
        public static IEnumerable<MenuItemGroupInfo> CalculateMenuGroups(IEnumerable<Ticket> tickets, IEnumerable<MenuItem> menuItems)
        {
            var query = from c in tickets.SelectMany(x => x.Orders.Select(y => new { Ticket = x, Order = y }))
                        join menuItem in menuItems on c.Order.MenuItemId equals menuItem.Id
                        group c by menuItem.GroupCode into grp
                        select new MenuItemGroupInfo
                        {
                            GroupName = grp.Key,
                            Quantity = grp.Sum(y => (y.Order.DecreaseInventory || y.Order.IncreaseInventory) ? y.Order.Quantity : 0),
                            Amount = grp.Sum(y => CalculateOrderTotal(y.Ticket, y.Order))
                        };

            var menuItemInfoGroups = query.ToList();

            var result = menuItemInfoGroups.OrderByDescending(x => x.Amount);

            var sum = menuItemInfoGroups.Sum(x => x.Amount);
            foreach (var menuItemInfoGroup in result)
            {
                if (sum > 0)
                    menuItemInfoGroup.Rate = (menuItemInfoGroup.Amount * 100) / sum;
                if (string.IsNullOrEmpty(menuItemInfoGroup.GroupName))
                    menuItemInfoGroup.GroupName = Localization.Properties.Resources.UndefinedWithBrackets;
            }

            var qsum = menuItemInfoGroups.Sum(x => x.Quantity);
            foreach (var menuItemInfoGroup in result)
            {
                if (qsum > 0)
                    menuItemInfoGroup.QuantityRate = (menuItemInfoGroup.Quantity * 100) / qsum;
                if (string.IsNullOrEmpty(menuItemInfoGroup.GroupName))
                    menuItemInfoGroup.GroupName = Localization.Properties.Resources.UndefinedWithBrackets;
            }

            return result;
        }

        public static IEnumerable<MenuItemSellInfo> CalculateMenuItems(IEnumerable<Ticket> tickets, IEnumerable<MenuItem> menuItems)
        {
            var menuItemSellInfos =
                from c in tickets.SelectMany(x => x.Orders.Where(y => y.DecreaseInventory).Select(y => new { Ticket = x, Order = y }))
                join menuItem in menuItems on c.Order.MenuItemId equals menuItem.Id
                group c by menuItem.Name into grp
                select new MenuItemSellInfo { Name = grp.Key, Quantity = grp.Sum(y => y.Order.Quantity), Amount = grp.Sum(y => CalculateOrderTotal(y.Ticket, y.Order)) };

            var result = menuItemSellInfos.ToList().OrderByDescending(x => x.Quantity);

            return result;
        }

        public static IEnumerable<MenuItemSellInfo> CalculateReturnedItems(IEnumerable<Ticket> tickets, IEnumerable<MenuItem> menuItems)
        {
            var menuItemSellInfos =
                from c in tickets.SelectMany(x => x.Orders.Where(y => y.IncreaseInventory).Select(y => new { Ticket = x, Order = y }))
                join menuItem in menuItems on c.Order.MenuItemId equals menuItem.Id
                group c by menuItem.Name into grp
                select new MenuItemSellInfo { Name = grp.Key, Quantity = grp.Sum(y => y.Order.Quantity), Amount = grp.Sum(y => CalculateOrderTotal(y.Ticket, y.Order)) };

            var result = menuItemSellInfos.ToList().OrderByDescending(x => x.Quantity);

            return result;
        }

        public static IEnumerable<MenuItemSellInfo> CalculatePortionsItems(IEnumerable<Ticket> tickets, MenuItem menuItem)
        {
            var menuItems = new List<MenuItem> { menuItem };

            var menuItemSellInfos =
                                   from c in tickets.SelectMany(x => x.Orders
                                                                      .Where(y => y.DecreaseInventory && y.MenuItemName == menuItem.Name)
                                                                      .Select(y => new { Ticket = x, Order = y }))
                                   join menuI in menuItems on c.Order.MenuItemId equals menuI.Id
                                   group c by c.Order.PortionName
                                       into grp
                                       select new MenuItemSellInfo
                                           {
                                               Name = "\t." + grp.Key,
                                               Quantity = grp.Sum(y => y.Order.Quantity),
                                               Amount = grp.Sum(y => CalculateOrderTotal(y.Ticket, y.Order))
                                           };
            var result = menuItemSellInfos.ToList().OrderByDescending(x => x.Quantity);

            return result;
        }

        public static decimal CalculateOrderTotal(Ticket ticket, Order order)
        {
            var discount = ticket.GetPreTaxServicesTotal();
            if (discount != 0)
            {
                var tsum = ticket.GetPlainSum();
                var rate = tsum > 0 ? (discount * 100) / tsum : 100;
                var tiTotal = order.GetTotal();
                var itemDiscount = (tiTotal * rate) / 100;
                return tiTotal + itemDiscount;
            }
            return order.GetTotal();
        }
    }
}