using System.Collections.Generic;
using System.ComponentModel.Composition;
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
        }

        protected decimal ExchangeRate { get; set; }
        public Ticket SelectedTicket { get; set; }
        public IList<Selector> Selectors { get; set; }
        public decimal SelectedTotal { get { return decimal.Round(Selectors.Sum(x => x.SelectedQuantity * x.Price), 2); } }
        public decimal RemainingTotal { get { return decimal.Round(Selectors.Sum(x => x.RemainingQuantity * x.Price), 2); } }

        public void UpdateTicket(Ticket ticket)
        {
            SelectedTicket = ticket;
            Selectors.Clear();
            UpdateSelectors();

            foreach (var paidItem in SelectedTicket.PaidItems)
            {
                var mi = Selectors.SingleOrDefault(x => x.Key == paidItem.Key);
                if (mi != null) mi.AddPaidItem(paidItem);
            }
        }

        private void UpdateSelector(Order order, decimal serviceAmount, decimal sum)
        {
            var selector = Selectors.FirstOrDefault(x => x.Key == GetKey(order));
            if (selector == null)
            {
                selector = new Selector { Key = GetKey(order) };
                selector.Description = order.MenuItemName + order.GetPortionDesc();
                Selectors.Add(selector);
            }
            selector.Quantity += order.Quantity;
            selector.Price = GetPrice(order, serviceAmount, sum, ExchangeRate);
        }

        private decimal GetPrice(Order order, decimal serviceAmount, decimal sum, decimal exchangeRate)
        {
            var result = order.GetItemPrice();
            result += (result * serviceAmount) / sum;
            if (!order.TaxIncluded) result += order.TaxAmount;
            result = result / exchangeRate;
            return result;
        }

        private static string GetKey(Order order)
        {
            return string.Format(Keyformat, order.MenuItemId, order.GetItemPrice());
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

        public void PersistTicket()
        {
            PersistSelectedItems();
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

            foreach (var order in SelectedTicket.Orders)
            {
                UpdateSelector(order, serviceAmount, sum);
            }
        }

        public void ClearSelection()
        {
            foreach (var selector in Selectors)
            {
                selector.ClearSelection();
            }
        }
    }
}