using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PaymentModule
{
    public class OrderSelector
    {
        private const string Keyformat = "{0}_{1}";

        public OrderSelector()
        {
            Selectors = new List<Selector>();
            ExchangeRate = 1;
            SelectedTicket = Ticket.Empty;
        }

        public decimal ExchangeRate { get; set; }
        public Ticket SelectedTicket { get; set; }
        public IList<Selector> Selectors { get; set; }
        public decimal SelectedTotal { get { return decimal.Round(Selectors.Sum(x => x.SelectedQuantity * x.Price), 2); } }
        public decimal RemainingTotal { get { return decimal.Round(Selectors.Sum(x => x.RemainingQuantity * x.Price), 2); } }
        protected decimal AutoRoundValue { get; set; }

        public void UpdateTicket(Ticket ticket)
        {
            SelectedTicket = ticket;
            Selectors.Clear();
            if (SelectedTicket.RemainingAmount > 0)
            {
                UpdateSelectors();
                foreach (var paidItem in SelectedTicket.PaidItems)
                {
                    var mi = Selectors.SingleOrDefault(x => x.Key == paidItem.Key);
                    if (mi != null) mi.AddPaidItem(paidItem);
                }
            }
        }

        private void UpdateSelector(Order order, decimal serviceAmount, decimal sum, bool taxIncluded)
        {
            var selector = Selectors.FirstOrDefault(x => x.Key == GetKey(order));
            if (selector == null)
            {
                selector = new Selector { Key = GetKey(order), Description = order.MenuItemName + order.GetPortionDesc() };
                Selectors.Add(selector);
            }
            selector.Quantity += order.Quantity;
            selector.Price = GetPrice(order, serviceAmount, sum, ExchangeRate, taxIncluded);
        }

        private static decimal GetPrice(Order order, decimal serviceAmount, decimal sum, decimal exchangeRate, bool taxIncluded)
        {
            var result = order.GetPrice();
            var tax = taxIncluded ? 0 : order.GetTaxAmount(false, sum, serviceAmount);
            if (serviceAmount != 0 && sum != 0) result += (result * serviceAmount) / sum;
            result += tax;
            result = result / exchangeRate;
            return result;
        }

        private static string GetKey(Order order)
        {
            return string.Format(Keyformat, order.MenuItemId, order.GetPrice());
        }

        public void Select(Selector selector)
        {
            selector.Select();
        }

        public void Select(int id, decimal price)
        {
            Select(Selectors.SingleOrDefault(x => x.Key == string.Format(Keyformat, id, price)));
        }

        public void PersistSelectedItems()
        {
            foreach (var selector in Selectors)
            {
                selector.PersistSelected();
            }
        }

        public void UpdateSelectedTicketPaidItems()
        {
            SelectedTicket.PaidItems.Clear();
            Selectors.SelectMany(x => x.GetSelectedItems()).ToList().ForEach(x => SelectedTicket.PaidItems.Add(x));
        }

        public void PersistTicket()
        {
            SelectedTicket.PaidItems.Clear();
            Selectors.SelectMany(x => x.GetPaidItems()).ToList().ForEach(x => SelectedTicket.PaidItems.Add(x));
        }

        public void UpdateExchangeRate(decimal exchangeRate)
        {
            ExchangeRate = exchangeRate;
            Selectors.ToList().ForEach(x => x.Quantity = 0);
            UpdateSelectors();
        }

        private void UpdateSelectors()
        {
            var serviceAmount = SelectedTicket.GetPreTaxServicesTotal() + SelectedTicket.GetPostTaxServicesTotal();
            var sum = SelectedTicket.GetPlainSum();

            foreach (var order in SelectedTicket.Orders.Where(x => x.CalculatePrice))
            {
                UpdateSelector(order, serviceAmount, sum, SelectedTicket.TaxIncluded);
            }

            RoundSelectors();
        }

        public void ClearSelection()
        {
            foreach (var selector in Selectors)
            {
                selector.ClearSelection();
            }
        }

        public void UpdateAutoRoundValue(decimal d)
        {
            AutoRoundValue = d;
            RoundSelectors();
        }

        private void RoundSelectors()
        {
            if (AutoRoundValue != 0 && RemainingTotal > 0)
            {
                var amount = 0m;
                foreach (var selector in Selectors)
                {
                    var price = selector.Price;
                    var newPrice = decimal.Round(price / AutoRoundValue, MidpointRounding.AwayFromZero) * AutoRoundValue;
                    selector.Price = newPrice;
                    amount += (newPrice * selector.RemainingQuantity);
                }
                var mLast = Selectors.Where(x => x.RemainingQuantity > 0).OrderBy(x => x.RemainingPrice).First();
                mLast.Price += ((SelectedTicket.GetSum() / ExchangeRate) - amount) / mLast.RemainingQuantity;
            }
        }

        public decimal GetRemainingAmount()
        {
            return SelectedTicket.GetRemainingAmount();
        }

        public IEnumerable<PaidItem> GetSelectedItems()
        {
            return Selectors.SelectMany(x => x.GetSelectedItems());
        }

        public decimal GetSelectedAmount()
        {
            var result = SelectedTotal;
            var remaining = GetRemainingAmount();
            if (result > remaining) result = remaining;
            return result;
        }
    }
}