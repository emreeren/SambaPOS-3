using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Settings;

namespace Samba.Domain.Models.Tickets
{
    public class Ticket : Entity, ICacheable
    {
        public Ticket()
            : this(0)
        {

        }

        public Ticket(int ticketId)
        {
            Id = ticketId;
            Date = DateTime.Now;
            LastPaymentDate = DateTime.Now;
            LastOrderDate = DateTime.Now;
            LastUpdateTime = DateTime.Now;

            _orders = new List<Order>();
            _paidItems = new List<PaidItem>();
            _calculations = new List<Calculation>();
            _payments = new List<Payment>();
            _ticketResources = new List<TicketResource>();
        }

        private static Ticket _emptyTicket;
        public static Ticket Empty
        {
            get
            {
                return _emptyTicket ?? (_emptyTicket = new Ticket
                                                           {
                                                               AccountTransactions = new AccountTransactionDocument()
                                                           });
            }
        }

        private bool _shouldLock;
        public DateTime LastUpdateTime { get; set; }

        private string _ticketNumber;
        public string TicketNumber
        {
            get { return _ticketNumber; }
            set
            {
                _ticketNumber = value;
                if (AccountTransactions != null)
                    AccountTransactions.Name = string.Format("Ticket Transaction [{0}]", TicketNumber);
            }
        }

        public DateTime Date { get; set; }
        public DateTime LastOrderDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public bool IsPaid { get; set; }
        public decimal RemainingAmount { get; set; }

        public int DepartmentId { get; set; }
        public string Note { get; set; }
        public bool Locked { get; set; }

        public decimal TotalAmount { get; set; }

        public int AccountId { get; set; }
        public int AccountTemplateId { get; set; }
        public string AccountName { get; set; }

        public string TicketTags { get; set; }

        public virtual AccountTransactionDocument AccountTransactions { get; set; }

        private IList<TicketResource> _ticketResources;
        public virtual IList<TicketResource> TicketResources
        {
            get { return _ticketResources; }
            set { _ticketResources = value; }
        }

        private IList<Order> _orders;
        public virtual IList<Order> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        }

        private IList<Calculation> _calculations;
        public virtual IList<Calculation> Calculations
        {
            get { return _calculations; }
            set { _calculations = value; }
        }

        private IList<Payment> _payments;
        public virtual IList<Payment> Payments
        {
            get { return _payments; }
            set { _payments = value; }
        }

        private IList<PaidItem> _paidItems;
        public virtual IList<PaidItem> PaidItems
        {
            get { return _paidItems; }
            set { _paidItems = value; }
        }

        private IList<TicketTagValue> _ticketTagValues;
        internal IList<TicketTagValue> TicketTagValues
        {
            get { return _ticketTagValues ?? (_ticketTagValues = JsonHelper.Deserialize<List<TicketTagValue>>(TicketTags)); }
        }

        public IList<TicketTagValue> GetTicketTagValues()
        {
            return TicketTagValues;
        }

        public Order AddOrder(AccountTransactionTemplate template, string userName, MenuItem menuItem, MenuItemPortion portion, string priceTag)
        {
            Locked = false;
            var tif = new Order();
            tif.UpdateMenuItem(userName, menuItem, portion, priceTag, 1);
            tif.AccountTransactionTemplateId = template.Id;
            if (AccountTransactions.AccountTransactions.SingleOrDefault(x => x.AccountTransactionTemplateId == template.Id) == null)
            {
                var transaction = AccountTransaction.Create(template);
                transaction.UpdateAccounts(AccountTemplateId, AccountId);
                AccountTransactions.AccountTransactions.Add(transaction);
            }

            if (tif.AccountTransactionTaxTemplateId > 0
                && AccountTransactions.AccountTransactions
                    .SingleOrDefault(x => x.AccountTransactionTemplateId == tif.AccountTransactionTaxTemplateId) == null)
            {
                var transaction = AccountTransaction.Create(menuItem.TaxTemplate.AccountTransactionTemplate);
                transaction.UpdateAccounts(AccountTemplateId, AccountId);
                AccountTransactions.AccountTransactions.Add(transaction);
            }
            Orders.Add(tif);
            return tif;
        }

        public void AddPayment(PaymentTemplate paymentTemplate, Account account, decimal amount, int userId)
        {
            var transaction = AccountTransaction.Create(paymentTemplate.AccountTransactionTemplate);
            transaction.Amount = amount;
            transaction.SetTargetAccount(account.AccountTemplateId, account.Id);
            transaction.UpdateAccounts(AccountTemplateId, AccountId);
            AccountTransactions.AccountTransactions.Add(transaction);
            var payment = new Payment { AccountTransaction = transaction, Amount = amount, Name = account.Name, PaymentTemplateId = paymentTemplate.Id };
            Payments.Add(payment);

            LastPaymentDate = DateTime.Now;
            RemainingAmount = GetRemainingAmount();
            if (RemainingAmount == 0)
            {
                PaidItems.Clear();
            }
        }

        public void RemoveOrder(Order ti)
        {
            var transactionId = ti.AccountTransactionTemplateId;
            Orders.Remove(ti);
            if (!Orders.Any(x => x.AccountTransactionTemplateId == transactionId))
            {
                AccountTransactions.AccountTransactions.Where(x => x.AccountTransactionTemplateId == transactionId)
                       .ToList().ForEach(x => AccountTransactions.AccountTransactions.Remove(x));
            }
        }

        public void RemovePayment(Payment py)
        {
            Payments.Remove(py);
            AccountTransactions.AccountTransactions.Where(x => x.Id == py.AccountTransaction.Id).ToList().ForEach(x => AccountTransactions.AccountTransactions.Remove(x));
        }

        public void RemoveCalculation(Calculation py)
        {
            var transactionId = py.AccountTransactionTemplateId;
            Calculations.Remove(py);
            if (!Calculations.Any(x => x.AccountTransactionTemplateId == transactionId))
            {
                AccountTransactions.AccountTransactions.Where(x => x.AccountTransactionTemplateId == transactionId)
                       .ToList().ForEach(x => AccountTransactions.AccountTransactions.Remove(x));
            }
        }

        public int GetItemCount()
        {
            return Orders.Count();
        }

        public decimal GetSum()
        {
            var plainSum = GetPlainSum();
            var services = CalculateServices(Calculations.Where(x => !x.IncludeTax), plainSum);
            var tax = CalculateTax(plainSum, services);
            plainSum = plainSum + services + tax;
            services = CalculateServices(Calculations.Where(x => x.IncludeTax), plainSum);
            return (plainSum + services);
        }

        public decimal GetPreTaxServicesTotal()
        {
            var plainSum = GetPlainSum();
            return CalculateServices(Calculations.Where(x => !x.IncludeTax), plainSum);
        }

        public decimal GetPostTaxServicesTotal()
        {
            var plainSum = GetPlainSum();
            var postServices = CalculateServices(Calculations.Where(x => !x.IncludeTax), plainSum);
            var tax = CalculateTax(plainSum, postServices);
            return CalculateServices(Calculations.Where(x => x.IncludeTax), plainSum + postServices + tax);
        }

        public decimal CalculateTax(decimal plainSum, decimal preTaxServices)
        {
            var result = Orders.Where(x => !x.TaxIncluded && x.CalculatePrice).Sum(x => (x.TaxAmount + x.OrderTagValues.Sum(y => y.TaxAmount)) * x.Quantity);
            if (preTaxServices > 0)
                result += (result * preTaxServices) / plainSum;
            return result;
        }

        private decimal CalculateServices(IEnumerable<Calculation> calculations, decimal sum)
        {
            decimal totalAmount = 0;
            var currentSum = sum;

            foreach (var calculation in calculations.OrderBy(x => x.Order))
            {
                if (calculation.CalculationType == 0)
                {
                    calculation.CalculationAmount = calculation.Amount > 0 ? (sum * calculation.Amount) / 100 : 0;
                }
                else if (calculation.CalculationType == 1)
                {
                    calculation.CalculationAmount = calculation.Amount > 0 ? (currentSum * calculation.Amount) / 100 : 0;
                }
                else if (calculation.CalculationType == 3)
                {
                    if (calculation.Amount == currentSum) calculation.Amount = 0;
                    else if (calculation.DecreaseAmount && calculation.Amount > currentSum)
                        calculation.Amount = 0;
                    else if (!calculation.DecreaseAmount && calculation.Amount < currentSum)
                        calculation.Amount = 0;
                    else
                        calculation.CalculationAmount = calculation.Amount - currentSum;
                }
                else if (calculation.CalculationType == 4)
                {
                    if (calculation.Amount > 0)
                        calculation.CalculationAmount = (decimal.Round(currentSum / calculation.Amount, MidpointRounding.AwayFromZero) * calculation.Amount) - currentSum;
                    else // eğer yuvarlama eksi olarak verildiyse hep aşağı yuvarlar
                        calculation.CalculationAmount = (Math.Truncate(currentSum / calculation.Amount) * calculation.Amount) - currentSum;

                    if (calculation.DecreaseAmount && calculation.CalculationAmount > 0) calculation.CalculationAmount = 0;
                    if (!calculation.DecreaseAmount && calculation.CalculationAmount < 0) calculation.CalculationAmount = 0;
                }
                else calculation.CalculationAmount = calculation.Amount;

                calculation.CalculationAmount = Decimal.Round(calculation.CalculationAmount, LocalSettings.Decimals);
                if (calculation.DecreaseAmount && calculation.CalculationAmount > 0) calculation.CalculationAmount = 0 - calculation.CalculationAmount;

                totalAmount += calculation.CalculationAmount;
                currentSum += calculation.CalculationAmount;

                //var s = service;
                //var transaction = AccountTransactions.AccountTransactions.Single(x => x.AccountTransactionTemplateId == s.AccountTransactionTemplateId);
                ////todo: Mutlak değeri yazmak çoğu durumda doğru ancak bazı durumlarda hesapların ters çevrilmesi gerekiyor. İncele
                //// Çözüm 1: İskonto olarak işaretli hesaplamaların adisyonun tutarını arttırıcı ya da tam tersi işleme neden olması engellendi.
                //transaction.Amount = Math.Abs(service.CalculationAmount);

                if (calculation.Amount == 0)
                {
                    Calculations.Remove(calculation);
                }

                UpdateCalculationTransaction(calculation, Math.Abs(calculation.CalculationAmount));
            }

            return decimal.Round(totalAmount, LocalSettings.Decimals);
        }

        public void AddCalculation(CalculationTemplate template, decimal amount)
        {
            var t = Calculations.SingleOrDefault(x => x.ServiceId == template.Id) ??
                    Calculations.SingleOrDefault(x => x.AccountTransactionTemplateId == template.AccountTransactionTemplate.Id);
            if (t == null)
            {
                t = new Calculation
                        {
                            Amount = amount,
                            Name = template.Name,
                            CalculationType = template.CalculationMethod,
                            ServiceId = template.Id,
                            IncludeTax = template.IncludeTax,
                            DecreaseAmount = template.DecreaseAmount,
                            Order = template.Order,
                            AccountTransactionTemplateId = template.AccountTransactionTemplate.Id,
                            AccountTransactionTemplate = template.AccountTransactionTemplate
                        };
                Calculations.Add(t);
            }
            else if (t.Amount == amount)
            {
                amount = 0;
            }
            else t.Amount = amount;
            t.Name = template.Name;
            if (amount == 0)
            {
                Calculations.Remove(t);
                UpdateCalculationTransaction(t, 0);
            }
        }

        public void UpdateCalculationTransaction(Calculation calculation, decimal amount)
        {
            var transaction = AccountTransactions.AccountTransactions.SingleOrDefault(x => x.AccountTransactionTemplateId == calculation.AccountTransactionTemplateId);
            if (transaction == null)
            {
                transaction = AccountTransaction.Create(calculation.AccountTransactionTemplate);
                transaction.Name = calculation.Name;
                transaction.UpdateAccounts(AccountTemplateId, AccountId);
                AccountTransactions.AccountTransactions.Add(transaction);
            }
            if (amount == 0)
            {
                AccountTransactions.AccountTransactions.Remove(
                    AccountTransactions.AccountTransactions.Single(x => x.AccountTransactionTemplateId == calculation.AccountTransactionTemplateId));
            }
            transaction.Amount = amount;
        }

        public decimal GetPlainSum()
        {
            return Orders.Sum(item => item.GetTotal());
        }

        public decimal GetPaymentAmount()
        {
            return Payments.Sum(x => x.Amount);
        }

        public decimal GetRemainingAmount()
        {
            var sum = GetSum();
            var payment = GetPaymentAmount();
            return decimal.Round(sum - payment, LocalSettings.Decimals);
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
            get { return TicketTagValues.Where(x => !string.IsNullOrEmpty(x.TagValue)).Count() > 0; }
        }

        public void CancelOrders(IEnumerable<Order> orders)
        {
            Locked = false;
            foreach (var order in orders.Where(order => order.Id == 0))
            {
                RemoveOrder(order);
            }
        }

        public bool CanRemoveSelectedOrders(IEnumerable<Order> items)
        {
            return (items.Sum(x => x.GetSelectedValue()) <= GetRemainingAmount());
        }

        public bool CanCancelSelectedOrders(IEnumerable<Order> selectedOrders)
        {
            var so = selectedOrders.ToList();
            return so.Count != 0 && !so.Any(x => x.Id > 0 || !Orders.Contains(x));
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
            if (_shouldLock || IsPaid) Locked = true;
            _shouldLock = false;
        }

        public static Ticket Create(Department department, Account account, IEnumerable<CalculationTemplate> calculationTemplates)
        {
            var ticket = new Ticket { DepartmentId = department.Id };

            ticket.AccountTemplateId = department.TicketTemplate.SaleTransactionTemplate.TargetAccountTemplateId;
            ticket.AccountTransactions = new AccountTransactionDocument();
            ticket.UpdateAccount(account);
            foreach (var calculationTemplate in calculationTemplates.OrderBy(x => x.Order))
            {
                ticket.AddCalculation(calculationTemplate, calculationTemplate.Amount);
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

        public string GetTagValue(string tagName)
        {
            var tag = TicketTagValues.SingleOrDefault(x => x.TagName == tagName);
            return tag != null ? tag.TagValue : "";
        }

        public void SetTagValue(string tagName, string tagValue)
        {
            var tag = TicketTagValues.SingleOrDefault(x => x.TagName == tagName);
            if (tag == null)
            {
                tag = new TicketTagValue { TagName = tagName, TagValue = tagValue };
                TicketTagValues.Add(tag);
            }
            else
                tag.TagValue = tagValue;

            if (string.IsNullOrEmpty(tag.TagValue))
                TicketTagValues.Remove(tag);

            TicketTags = JsonHelper.Serialize(TicketTagValues);
            _ticketTagValues = null;
        }

        public string GetTagData()
        {
            return string.Join("\r", TicketTagValues.Where(x => !string.IsNullOrEmpty(x.TagValue)).Select(x => string.Format("{0}: {1}", x.TagName, x.TagValue)));
        }

        public void UpdateResource(int resourceTemplateId, int resourceId, string resourceName, int accountId, string resourceCustomData)
        {
            var r = TicketResources.SingleOrDefault(x => x.ResourceTemplateId == resourceTemplateId);
            if (r == null && resourceId > 0)
            {
                TicketResources.Add(new TicketResource { ResourceId = resourceId, ResourceName = resourceName, ResourceTemplateId = resourceTemplateId, AccountId = accountId, ResourceCustomData = resourceCustomData });
            }
            else if (resourceId > 0)
            {
                r.AccountId = accountId;
                r.ResourceId = resourceId;
                r.ResourceName = resourceName;
                r.ResourceTemplateId = resourceTemplateId;

            }
            else if (r != null && resourceId == 0)
                TicketResources.Remove(r);
        }

        public void UpdateAccount(Account account)
        {
            if (account == null) return;
            foreach (var transaction in AccountTransactions.AccountTransactions)
            {
                transaction.UpdateAccounts(AccountTemplateId, account.Id);
            }
            AccountId = account.Id;
            AccountTemplateId = account.AccountTemplateId;
            AccountName = account.Name;
        }

        public void Recalculate(decimal autoRoundValue, int userId)
        {
            //if (autoRoundValue != 0)
            //{
            //    AddTicketDiscount(template, 0, userId);
            //    var ramount = GetRemainingAmount();
            //    if (ramount > 0)
            //    {
            //        decimal damount;
            //        if (autoRoundValue > 0)
            //            damount = decimal.Round(ramount / autoRoundValue, MidpointRounding.AwayFromZero) * autoRoundValue;
            //        else // eğer yuvarlama eksi olarak verildiyse hep aşağı yuvarlar
            //            damount = Math.Truncate(ramount / autoRoundValue) * autoRoundValue;
            //        AddTicketDiscount(template, ramount - damount, userId);
            //    }
            //    else if (ramount < 0)
            //    {
            //        AddTicketDiscount(template, ramount, userId);
            //    }
            //}

            //AccountTransactions.AccountTransactions.Where(x => !Orders.Any(y => y.AccountTransactionTemplateId == x.AccountTransactionTemplateId))
            //    .ToList().ForEach(x => AccountTransactions.AccountTransactions.Remove(x));

            if (Orders.Count > 0)
            {
                var transactionGroup = Orders.GroupBy(x => x.AccountTransactionTemplateId);
                foreach (var transactionItem in transactionGroup)
                {
                    var t = transactionItem;
                    var transaction = AccountTransactions.AccountTransactions.Single(x => x.AccountTransactionTemplateId == t.Key);
                    transaction.UpdateAccounts(AccountTemplateId, AccountId);
                    transaction.Amount = t.Sum(x => x.GetTotal() - x.GetTotalTaxAmount());
                }

                var taxGroup = Orders.Where(x => x.AccountTransactionTaxTemplateId > 0).GroupBy(x => x.AccountTransactionTaxTemplateId);

                foreach (var taxGroupItem in taxGroup)
                {
                    var tg = taxGroupItem;
                    var transaction = AccountTransactions.AccountTransactions.Single(x => x.AccountTransactionTemplateId == tg.Key);
                    transaction.UpdateAccounts(AccountTemplateId, AccountId);
                    transaction.Amount = tg.Sum(x => x.GetTotalTaxAmount());
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
                newItem.OrderTagValues.ToList().ForEach(x => x.Id = 0);
                newItem.OrderTagValues.ToList().ForEach(x => x.TicketId = 0);
                newItem.OrderTagValues.ToList().ForEach(x => x.OrderId = 0);
                newItem.Id = 0;
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
            selectedOrders.ToList().ForEach(RemoveOrder);
        }

        public bool IsTaggedWith(string tagName)
        {
            return !string.IsNullOrEmpty(GetTagValue(tagName));
        }

        public bool CanCloseTicket()
        {
            return (GetRemainingAmount() <= 0 || TicketResources.Count > 0 ||
                 IsTagged || Orders.Count == 0);
        }

        public decimal GetCalculationTotal(string s)
        {
            return Calculations.Where(x => string.IsNullOrEmpty(s) || x.Name == s).Sum(x => x.CalculationAmount);
        }

        public string GetResourceName(int resourceTemplateId)
        {
            var tr = TicketResources.FirstOrDefault(x => x.ResourceTemplateId == resourceTemplateId);
            if (tr != null) return tr.ResourceName;
            return "";
        }

        public decimal GetOrderStateTotal(string s)
        {
            return Orders.Where(x => x.OrderStateGroupName == s).Sum(x => x.GetItemValue());
        }
    }
}
