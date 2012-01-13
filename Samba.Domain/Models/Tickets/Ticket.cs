using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Settings;

namespace Samba.Domain.Models.Tickets
{
    public class Ticket : IEntity
    {
        public Ticket()
            : this(0, "")
        {

        }

        public Ticket(int ticketId)
            : this(ticketId, "")
        {

        }

        public Ticket(int ticketId, string locationName)
        {
            Id = ticketId;
            Date = DateTime.Now;
            LastPaymentDate = DateTime.Now;
            LastOrderDate = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            LocationName = locationName;
            PrintJobData = "";

            _orders = new List<Order>();
            _payments = new List<Payment>();
            _discounts = new List<Discount>();
            _paidItems = new List<PaidItem>();
            _services = new List<Service>();
            _tags = new List<TicketTagValue>();
        }

        private static Ticket _emptyTicket;
        public static Ticket Empty { get { return _emptyTicket ?? (_emptyTicket = new Ticket()); } }

        private bool _shouldLock;
        private Dictionary<int, int> _printCounts;

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string TicketNumber { get; set; }
        public string PrintJobData { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastOrderDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public string LocationName { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public bool IsPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int DepartmentId { get; set; }
        public string Note { get; set; }
        public bool Locked { get; set; }

        private IList<Order> _orders;
        public virtual IList<Order> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        }

        private IList<Payment> _payments;
        public virtual IList<Payment> Payments
        {
            get { return _payments; }
            set { _payments = value; }
        }

        private IList<Discount> _discounts;
        public virtual IList<Discount> Discounts
        {
            get { return _discounts; }
            set { _discounts = value; }
        }

        private IList<Service> _services;
        public virtual IList<Service> Services
        {
            get { return _services; }
            set { _services = value; }
        }

        private IList<TicketTagValue> _tags;
        public virtual IList<TicketTagValue> Tags
        {
            get { return _tags; }
            set { _tags = value; }
        }

        private IList<PaidItem> _paidItems;
        public virtual IList<PaidItem> PaidItems
        {
            get { return _paidItems; }
            set { _paidItems = value; }
        }

        public Order AddOrder(string userName, MenuItem menuItem, string portionName, string priceTag)
        {
            Locked = false;
            var tif = new Order();
            tif.UpdateMenuItem(userName, menuItem, portionName, priceTag, 1);
            Orders.Add(tif);
            return tif;
        }

        public Payment AddPayment(DateTime date, decimal amount, PaymentType paymentType, int userId)
        {
            var result = new Payment { Amount = amount, Date = date, PaymentType = (int)paymentType, UserId = userId };
            Payments.Add(result);
            LastPaymentDate = DateTime.Now;
            RemainingAmount = GetRemainingAmount();
            if (RemainingAmount == 0)
            {
                PaidItems.Clear();
            }
            return result;
        }

        public void RemoveOrder(Order ti)
        {
            Orders.Remove(ti);
        }

        public int GetItemCount()
        {
            return Orders.Count();
        }

        public decimal GetSumWithoutTax()
        {
            var sum = GetPlainSum();
            sum -= GetDiscountAndRoundingTotal();
            return sum;
        }

        public decimal GetSum()
        {
            var plainSum = GetPlainSum();
            var discount = CalculateDiscounts(Discounts.Where(x => x.DiscountType == (int)DiscountType.Percent), plainSum);
            var tax = CalculateTax(plainSum, discount);
            var services = CalculateServices(Services, plainSum - discount, tax);
            return (plainSum - discount + services + tax) - Discounts.Where(x => x.DiscountType != (int)DiscountType.Percent).Sum(x => x.Amount);
        }

        public decimal CalculateTax()
        {
            return CalculateTax(GetPlainSum(), GetDiscountTotal());
        }

        private decimal CalculateTax(decimal plainSum, decimal discount)
        {
            var result = Orders.Where(x => !x.TaxIncluded && x.CalculatePrice).Sum(x => (x.TaxAmount + x.OrderTagValues.Sum(y => y.TaxAmount)) * x.Quantity);
            if (discount > 0)
                result -= (result * discount) / plainSum;
            return result;
        }

        public decimal GetDiscountAndRoundingTotal()
        {
            decimal sum = GetPlainSum();
            return CalculateDiscounts(Discounts, sum);
        }

        public decimal GetDiscountTotal()
        {
            decimal sum = GetPlainSum();
            return CalculateDiscounts(Discounts.Where(x => x.DiscountType == (int)DiscountType.Percent), sum);
        }

        public decimal GetRoundingTotal()
        {
            return CalculateDiscounts(Discounts.Where(x => x.DiscountType != (int)DiscountType.Percent), 0);
        }

        public decimal GetServicesTotal()
        {
            var plainSum = GetPlainSum();
            var discount = GetDiscountTotal();
            var tax = CalculateTax(plainSum, discount);
            return CalculateServices(Services, plainSum - discount, tax);
        }

        private static decimal CalculateServices(IEnumerable<Service> services, decimal sum, decimal tax)
        {
            decimal totalAmount = 0;
            var currentSum = sum;

            foreach (var service in services)
            {
                if (service.CalculationType == 0)
                {
                    service.CalculationAmount = service.Amount > 0 ? (sum * service.Amount) / 100 : 0;
                }
                else if (service.CalculationType == 1)
                {
                    service.CalculationAmount = service.Amount > 0 ? ((sum + tax) * service.Amount) / 100 : 0;
                }
                else if (service.CalculationType == 2)
                {
                    service.CalculationAmount = service.Amount > 0 ? (currentSum * service.Amount) / 100 : 0;
                }
                else service.CalculationAmount = service.Amount;

                service.CalculationAmount = Decimal.Round(service.CalculationAmount, LocalSettings.Decimals);
                totalAmount += service.CalculationAmount;
                currentSum += service.CalculationAmount;
            }

            return decimal.Round(totalAmount, LocalSettings.Decimals);
        }

        private decimal CalculateDiscounts(IEnumerable<Discount> discounts, decimal sum)
        {
            decimal totalDiscount = 0;
            foreach (var discount in discounts)
            {
                if (discount.DiscountType == (int)DiscountType.Percent)
                {
                    if (discount.OrderId == 0)
                        discount.DiscountAmount = discount.Amount > 0
                            ? (sum * discount.Amount) / 100 : 0;
                    else
                    {
                        var d = discount;
                        discount.DiscountAmount = discount.Amount > 0
                            ? (Orders.Single(x => x.Id == d.OrderId).GetTotal() * discount.Amount) / 100 : 0;
                    }
                }
                else discount.DiscountAmount = discount.Amount;

                discount.DiscountAmount = Decimal.Round(discount.DiscountAmount, LocalSettings.Decimals);
                totalDiscount += discount.DiscountAmount;
            }
            return decimal.Round(totalDiscount, LocalSettings.Decimals);
        }

        public void AddTicketDiscount(DiscountType type, decimal amount, int userId)
        {
            var c = Discounts.SingleOrDefault(x => x.DiscountType == (int)type);
            if (c == null)
            {
                c = new Discount { DiscountType = (int)type, Amount = amount };
                Discounts.Add(c);
            }
            if (amount == 0) Discounts.Remove(c);
            c.UserId = userId;
            c.Amount = amount;
        }

        public void AddService(int serviceId, int calculationMethod, decimal amount)
        {
            var t = Services.SingleOrDefault(x => x.ServiceId == serviceId);
            if (t == null)
            {
                t = new Service
                        {
                            Amount = amount,
                            CalculationType = calculationMethod,
                            ServiceId = serviceId
                        };
                Services.Add(t);
            }

            if (amount == 0)
                Services.Remove(t);
            t.Amount = amount;
        }

        public decimal GetPlainSum()
        {
            return Orders.Sum(item => item.GetTotal());
        }

        public decimal GetPaymentAmount()
        {
            return Payments.Sum(item => item.Amount);
        }

        public decimal GetRemainingAmount()
        {
            var sum = GetSum();
            var payment = GetPaymentAmount();
            return decimal.Round(sum - payment, LocalSettings.Decimals);
        }

        public decimal GetAccountPaymentAmount()
        {
            return Payments.Where(x => x.PaymentType != (int)PaymentType.Account).Sum(x => x.Amount);
        }

        public decimal GetAccountRemainingAmount()
        {
            return Payments.Where(x => x.PaymentType == (int)PaymentType.Account).Sum(x => x.Amount);
        }

        public string UserString
        {
            get { return Name; }
        }

        public bool CanSubmit
        {
            get { return !IsPaid; }
        }

        public bool IsTagged
        {
            get { return Tags.Where(x => !string.IsNullOrEmpty(x.TagValue)).Count() > 0; }
        }

        public void CancelOrder(Order item)
        {
            Locked = false;
            if (item.Id == 0) RemoveOrder(item);
        }

        public bool CanRemoveSelectedOrders(IEnumerable<Order> items)
        {
            return (items.Sum(x => x.GetSelectedValue()) <= GetRemainingAmount());
        }

        public bool CanCancelSelectedOrders(IEnumerable<Order> items)
        {
            if (items.Count() == 0) return false;
            foreach (var item in items)
            {
                if (!Orders.Contains(item)) return false;
                if (item.Id > 0) return false;
            }
            return true;
        }

        public IEnumerable<Order> GetUnlockedOrders()
        {
            return Orders.Where(x => !x.Locked).OrderBy(x => x.Id).ToList();
        }

        public void MergeOrdersAndUpdateOrderNumbers(int orderNumber)
        {
            LastOrderDate = DateTime.Now;
            IList<Order> newOrders = Orders.Where(x => !x.Locked && x.Id == 0).ToList();

            //sadece quantity = 1 olan satırlar birleştirilecek.
            var mergedOrders = newOrders.Where(x => x.Quantity != 1).ToList();
            var ids = mergedOrders.Select(x => x.MenuItemId).Distinct().ToArray();
            mergedOrders.AddRange(newOrders.Where(x => ids.Contains(x.MenuItemId) && x.Quantity == 1));
            foreach (var order in newOrders.Where(x => x.Quantity == 1 && !ids.Contains(x.MenuItemId)))
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
                        x.PortionName == ti.PortionName && x.CalculatePrice == ti.CalculatePrice);
                if (item == null) mergedOrders.Add(order);
                else item.Quantity += order.Quantity;
            }

            foreach (var order in newOrders.Where(order => !mergedOrders.Contains(order)))
            {
                RemoveOrder(order);
            }

            foreach (var item in Orders.Where(x => !x.Locked).Where(order => order.OrderNumber == 0))
            {
                item.OrderNumber = orderNumber;
            }
        }

        public void RequestLock()
        {
            _shouldLock = true;
        }

        public void LockTicket()
        {
            foreach (var order in Orders.Where(x => !x.Locked))
            {
                order.Locked = true;
            }
            if (_shouldLock) Locked = true;
            _shouldLock = false;
        }

        public static Ticket Create(Department department)
        {
            var ticket = new Ticket { DepartmentId = department.Id };
            foreach (var serviceTemplate in department.TicketTemplate.ServiceTemplates)
            {
                ticket.AddService(serviceTemplate.Id, serviceTemplate.CalculationMethod, serviceTemplate.Amount);
            }
            return ticket;
        }

        public Order CloneOrder(Order item)
        {
            Debug.Assert(_orders.Contains(item));
            var result = ObjectCloner.Clone(item);
            result.CreatedDateTime = DateTime.Now;
            result.Quantity = 0;
            _orders.Add(result);
            return result;
        }

        public bool DidPrintJobExecuted(int printerId)
        {
            return GetPrintCount(printerId) > 0;
        }

        public void AddPrintJob(int printerId)
        {
            if (_printCounts == null)
                _printCounts = CreatePrintCounts(PrintJobData);
            if (!_printCounts.ContainsKey(printerId))
                _printCounts.Add(printerId, 0);
            _printCounts[printerId]++;
            PrintJobData = string.Join("#", _printCounts.Select(x => string.Format("{0}:{1}", x.Key, x.Value)));
        }

        public int GetPrintCount(int id)
        {
            if (_printCounts == null)
                _printCounts = CreatePrintCounts(PrintJobData);
            return _printCounts.ContainsKey(id) ? _printCounts[id] : 0;
        }

        private static Dictionary<int, int> CreatePrintCounts(string pJobData)
        {
            try
            {
                return pJobData
                    .Split('#')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(item => item.Split(':').Where(x => !string.IsNullOrEmpty(x)).Select(x => Convert.ToInt32(x)).ToArray())
                    .ToDictionary(d => d[0], d => d[1]);
            }
            catch (Exception)
            {
                return new Dictionary<int, int>();
            }
        }

        public string GetTagValue(string tagName)
        {
            var tag = Tags.SingleOrDefault(x => x.TagName == tagName);
            return tag != null ? tag.TagValue : "";
        }

        public void SetTagValue(string tagName, string tagValue)
        {
            var tag = Tags.SingleOrDefault(x => x.TagName == tagName);
            if (tag == null)
            {
                tag = new TicketTagValue { TagName = tagName, TagValue = tagValue };
                Tags.Add(tag);
            }
            else
                tag.TagValue = tagValue;
            tag.DateTime = DateTime.Now;
        }

        public string GetTagData()
        {
            return string.Join("\r", Tags.Where(x => !string.IsNullOrEmpty(x.TagValue)).Select(x => string.Format("{0}: {1}", x.TagName, x.TagValue)));
        }

        public void UpdateAccount(Account account)
        {
            if (account == Account.Null)
            {
                AccountId = 0;
                AccountName = "";
            }
            else
            {
                AccountId = account.Id;
                AccountName = account.Name.Trim();
            }
        }

        public void Recalculate(decimal autoRoundValue, int userId)
        {
            if (autoRoundValue != 0)
            {
                AddTicketDiscount(DiscountType.Auto, 0, userId);
                var ramount = GetRemainingAmount();
                if (ramount > 0)
                {
                    decimal damount;
                    if (autoRoundValue > 0)
                        damount = decimal.Round(ramount / autoRoundValue, MidpointRounding.AwayFromZero) * autoRoundValue;
                    else // eğer yuvarlama eksi olarak verildiyse hep aşağı yuvarlar
                        damount = Math.Truncate(ramount / autoRoundValue) * autoRoundValue;
                    AddTicketDiscount(DiscountType.Auto, ramount - damount, userId);
                }
                else if (ramount < 0)
                {
                    AddTicketDiscount(DiscountType.Auto, ramount, userId);
                }
            }

            RemainingAmount = GetRemainingAmount();
            TotalAmount = GetSum();
        }

        public IEnumerable<Order> ExtractSelectedOrders(IEnumerable<Order> selectedOrders)
        {
            var newItems = new List<Order>();

            foreach (var order in selectedOrders)
            {
                Debug.Assert(order.SelectedQuantity > 0);
                Debug.Assert(Orders.Contains(order));
                if (order.SelectedQuantity >= order.Quantity) continue;
                var newItem = CloneOrder(order);
                newItem.Quantity = order.SelectedQuantity;
                order.Quantity -= order.SelectedQuantity;
                newItems.Add(newItem);
            }

            return newItems;
        }

        public void UpdateTax(TaxTemplate taxTemplate)
        {
            Orders.ToList().ForEach(x => x.UpdateTaxTemplate(taxTemplate));
        }

        public void RemoveOrders(IEnumerable<Order> selectedOrders)
        {
            selectedOrders.ToList().ForEach(x => Orders.Remove(x));
        }
    }
}
