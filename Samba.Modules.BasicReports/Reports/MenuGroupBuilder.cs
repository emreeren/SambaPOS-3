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
            var menuItemInfoGroups =
                from c in tickets.SelectMany(x => x.TicketItems.Select(y => new { Ticket = x, TicketItem = y }))
                join menuItem in menuItems on c.TicketItem.MenuItemId equals menuItem.Id
                group c by menuItem.GroupCode into grp
                select new MenuItemGroupInfo
                {
                    GroupName = grp.Key,
                    Quantity = grp.Sum(y => y.TicketItem.Quantity),
                    Amount = grp.Sum(y => CalculateTicketItemTotal(y.Ticket, y.TicketItem))
                };

            var result = menuItemInfoGroups.ToList().OrderByDescending(x => x.Amount);

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
                from c in tickets.SelectMany(x => x.TicketItems.Where(y => !y.Voided).Select(y => new { Ticket = x, TicketItem = y }))
                join menuItem in menuItems on c.TicketItem.MenuItemId equals menuItem.Id
                group c by menuItem.Name into grp
                select new MenuItemSellInfo { Name = grp.Key, Quantity = grp.Sum(y => y.TicketItem.Quantity), Amount = grp.Sum(y => CalculateTicketItemTotal(y.Ticket, y.TicketItem)) };

            var result = menuItemSellInfos.ToList().OrderByDescending(x => x.Quantity);

            return result;
        }


        public static decimal CalculateTicketItemTotal(Ticket ticket, TicketItem ticketItem)
        {
            var discount = ticket.GetDiscountAndRoundingTotal();
            if (discount != 0)
            {
                var tsum = ticket.GetSumWithoutTax() + discount;
                var rate = tsum > 0 ? (discount * 100) / tsum : 100;
                var tiTotal = ticketItem.GetTotal();
                var itemDiscount = (tiTotal * rate) / 100;
                return tiTotal - itemDiscount;
            }
            return ticketItem.GetTotal();
        }
    }
}