using System.Collections.Generic;
using System.Linq;

namespace Samba.Domain.Models.Tickets
{
    public static class OrderMerger
    {
        public static IEnumerable<Order> NewMerge(IEnumerable<Order> orders)
        {
            var result = new List<Order>();
            var source = orders.ToList();

            var bannedMenuItems = source.Where(x => x.Quantity > 1).Select(x => x.MenuItemId).Distinct();
            result.AddRange(source.Where(x => bannedMenuItems.Contains(x.MenuItemId)));
            result.ForEach(x => source.Remove(x));

            while (source.Any())
            {
                var order1 = source.First();
                source.Remove(order1);
                var matches = source.Where(x => CanMergeOrders(order1, x)).ToList();
                matches.ForEach(x => source.Remove(x));
                order1.Quantity += matches.Sum(x => x.Quantity);
                result.Add(order1);
            }

            return result;
        }

        public static IEnumerable<Order> Merge(IEnumerable<Order> orders)
        {
            return NewMerge(orders);
        }

        public static IEnumerable<Order> OldMerge(IEnumerable<Order> orders)
        {
            var mergedOrders = orders.Where(x => x.Quantity != 1).ToList();
            var ids = mergedOrders.Select(x => x.MenuItemId).Distinct().ToArray();
            mergedOrders.AddRange(orders.Where(x => ids.Contains(x.MenuItemId) && x.Quantity == 1));
            foreach (var order in orders.Where(x => x.Quantity == 1 && !ids.Contains(x.MenuItemId)))
            {
                var ti = order;
                if (order.OrderTagValues.Count > 0)
                {
                    mergedOrders.Add(order);
                    continue;
                }

                var item =
                    mergedOrders.SingleOrDefault(
                        x =>
                        x.OrderTagValues.Count == 0 && x.MenuItemId == ti.MenuItemId &&
                        x.PortionName == ti.PortionName && x.CalculatePrice == ti.CalculatePrice && x.Price == ti.Price);
                if (item == null) mergedOrders.Add(order);
                else
                {
                    item.Quantity += order.Quantity;
                    item.ResetSelectedQuantity();
                }
            }

            return mergedOrders;
        }

        public static bool CanMergeOrders(Order order1, Order order2)
        {
            if (order1.Quantity != order2.Quantity) return false;
            if (order1.Price != order2.Price) return false;
            if (order1.MenuItemId != order2.MenuItemId) return false;
            if (order1.PortionName != order2.PortionName) return false;
            if (order1.CalculatePrice != order2.CalculatePrice) return false;
            if (order1.IncreaseInventory != order2.IncreaseInventory) return false;
            if (order1.DecreaseInventory != order2.DecreaseInventory) return false;
            if (order1.OrderTagValues.Count > 0 || order2.OrderTagValues.Count > 0) return false;
            if (!OrderStatesEqual(order1, order2)) return false;
            return true;
        }

        private static bool OrderStatesEqual(Order order1, Order order2)
        {
            var order1OrderStateValues = order1.GetOrderStateValues().ToList();
            var order2OrderStateValues = order2.GetOrderStateValues().ToList();
            if (!order1OrderStateValues.Any() && !order2OrderStateValues.Any()) return true;
            if (order1OrderStateValues.Count() != order2OrderStateValues.Count()) return false;
            return order1OrderStateValues.All(ps => order2OrderStateValues.Any(cs => cs.Equals(ps)));
        }
    }
}