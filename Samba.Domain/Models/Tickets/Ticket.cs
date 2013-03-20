﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Helpers;
using Samba.Infrastructure.Settings;

namespace Samba.Domain.Models.Tickets
{
    public class Ticket : EntityClass, ICacheable
    {
        private bool _shouldLock;

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
            ExchangeRate = 1;

            _orders = new List<Order>();
            _paidItems = new List<PaidItem>();
            _calculations = new List<Calculation>();
            _payments = new List<Payment>();
            _changePayments = new List<ChangePayment>();
            _ticketEntities = new List<TicketEntity>();
        }

        private static Ticket _emptyTicket;
        public static Ticket Empty
        {
            get
            {
                return _emptyTicket ?? (_emptyTicket = new Ticket
                                                           {
                                                               TransactionDocument = new AccountTransactionDocument()
                                                           });
            }
        }

        public DateTime LastUpdateTime { get; set; }

        private string _ticketNumber;
        public string TicketNumber
        {
            get { return _ticketNumber; }
            set
            {
                _ticketNumber = value;
                if (TransactionDocument != null)
                    TransactionDocument.Name = string.Format("Ticket Transaction [{0}]", TicketNumber);
            }
        }

        public DateTime Date { get; set; }
        public DateTime LastOrderDate { get; set; }
        public DateTime LastPaymentDate { get; set; }

        public bool IsClosed { get; set; }
        public bool IsLocked { get; set; }
        public void UnLock() { if (!IsClosed) IsLocked = false; }
        public void Lock() { IsLocked = true; }
        public void Close()
        {
            if (RemainingAmount == 0 && !HasActiveTimers())
                IsClosed = true;
        }

        public decimal RemainingAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public int DepartmentId { get; set; }
        public int TicketTypeId { get; set; }
        public string Note { get; set; }

        public string TicketTags
        {
            get { return _ticketTags; }
            set
            {
                _ticketTags = value;
                _ticketTagValues = null;
            }
        }

        public string TicketStates
        {
            get { return _ticketStates; }
            set
            {
                _ticketStates = value;
                _ticketStateValues = null;
            }
        }

        public decimal ExchangeRate { get; set; }

        public bool TaxIncluded { get; set; }

        public virtual AccountTransactionDocument TransactionDocument { get; set; }

        private IList<TicketEntity> _ticketEntities;
        public virtual IList<TicketEntity> TicketEntities
        {
            get { return _ticketEntities; }
            set { _ticketEntities = value; }
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

        private IList<ChangePayment> _changePayments;
        public virtual IList<ChangePayment> ChangePayments
        {
            get { return _changePayments; }
            set { _changePayments = value; }
        }

        private IList<PaidItem> _paidItems;
        public virtual IList<PaidItem> PaidItems
        {
            get { return _paidItems; }
            set { _paidItems = value; }
        }

        private IList<TicketTagValue> _ticketTagValues;
        private IList<TicketTagValue> TicketTagValues
        {
            get { return _ticketTagValues ?? (_ticketTagValues = JsonHelper.Deserialize<List<TicketTagValue>>(TicketTags)); }
        }

        private IList<TicketStateValue> _ticketStateValues;
        private string _ticketTags;
        private string _ticketStates;

        private IList<TicketStateValue> TicketStateValues
        {
            get { return _ticketStateValues ?? (_ticketStateValues = JsonHelper.Deserialize<List<TicketStateValue>>(TicketStates)); }
        }

        public IList<TicketTagValue> GetTicketTagValues()
        {
            return TicketTagValues;
        }

        public IEnumerable<TicketStateValue> GetTicketStateValues()
        {
            return TicketStateValues;
        }

        public IEnumerable<AccountData> GetTicketAccounts()
        {
            return TicketEntities.Select(x => new AccountData(x.AccountTypeId, x.AccountId));
        }

        public Order AddOrder(AccountTransactionType template, Department department, string userName, MenuItem menuItem, IList<TaxTemplate> taxTemplates, MenuItemPortion portion, string priceTag, ProductTimer timer)
        {
            UnLock();
            var order = new Order();
            order.UpdateMenuItem(userName, menuItem, taxTemplates, portion, priceTag, 1);
            order.AccountTransactionTypeId = template.Id;
            order.WarehouseId = department.WarehouseId;
            order.DepartmentId = department.Id;
            TransactionDocument.AddSingletonTransaction(template.Id, template, GetTicketAccounts());

            if (taxTemplates != null)
            {
                foreach (var taxTemplate in taxTemplates)
                {
                    TransactionDocument.AddSingletonTransaction(taxTemplate.AccountTransactionType.Id,
                                               taxTemplate.AccountTransactionType,
                                               GetTicketAccounts());
                }
            }

            order.UpdateProductTimer(timer);
            Orders.Add(order);
            return order;
        }

        public void AddPayment(PaymentType paymentType, Account account, decimal amount, decimal exchangeRate, int userId)
        {
            var transaction = TransactionDocument.AddNewTransaction(paymentType.AccountTransactionType, GetTicketAccounts(), amount, exchangeRate);
            var payment = new Payment { AccountTransaction = transaction, Amount = amount, Name = account.Name, PaymentTypeId = paymentType.Id };
            Payments.Add(payment);
            LastPaymentDate = DateTime.Now;
            RemainingAmount = GetRemainingAmount();
            if (RemainingAmount == 0)
            {
                PaidItems.Clear();
            }
        }

        public void AddChangePayment(ChangePaymentType changePaymentType, Account account, decimal amount, decimal exchangeRate, int userId)
        {
            var transaction = TransactionDocument.AddNewTransaction(changePaymentType.AccountTransactionType, GetTicketAccounts(), amount, exchangeRate);
            var payment = new ChangePayment { AccountTransaction = transaction, Amount = amount, Name = account.Name, ChangePaymentTypeId = changePaymentType.Id };
            ChangePayments.Add(payment);
        }

        public void RemoveOrder(Order order)
        {
            var transactionId = order.AccountTransactionTypeId;
            Orders.Remove(order);
            if (Orders.All(x => x.AccountTransactionTypeId != transactionId))
            {
                TransactionDocument.AccountTransactions.Where(x => x.AccountTransactionTypeId == transactionId)
                       .ToList().ForEach(x => TransactionDocument.AccountTransactions.Remove(x));
            }
        }

        public void RemovePayment(Payment py)
        {
            Payments.Remove(py);
            TransactionDocument.AccountTransactions.Where(x => x.Id == py.AccountTransaction.Id).ToList().ForEach(x => TransactionDocument.AccountTransactions.Remove(x));
        }

        public void RemoveChangePayment(ChangePayment py)
        {
            ChangePayments.Remove(py);
            TransactionDocument.AccountTransactions.Where(x => x.Id == py.AccountTransaction.Id).ToList().ForEach(x => TransactionDocument.AccountTransactions.Remove(x));
        }

        public void RemoveCalculation(Calculation py)
        {
            var transactionId = py.AccountTransactionTypeId;
            Calculations.Remove(py);
            if (Calculations.All(x => x.AccountTransactionTypeId != transactionId))
            {
                TransactionDocument.AccountTransactions.Where(x => x.AccountTransactionTypeId == transactionId)
                       .ToList().ForEach(x => TransactionDocument.AccountTransactions.Remove(x));
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
            var tax = TaxIncluded ? 0 : CalculateTax(plainSum, services);
            plainSum = plainSum + services + tax;
            services = CalculateServices(Calculations.Where(x => x.IncludeTax), plainSum);
            return (plainSum + services);
        }

        public decimal GetSum(int taxTempleteAccountTransactionTypeId)
        {
            var plainSum = GetPlainSum(taxTempleteAccountTransactionTypeId);
            var services = CalculateServices(Calculations.Where(x => !x.IncludeTax), plainSum);
            var tax = TaxIncluded ? 0 : CalculateTax(plainSum, services);
            plainSum = plainSum + services + tax;
            services = CalculateServices(Calculations.Where(x => x.IncludeTax), plainSum);
            return (plainSum + services);
        }

        public decimal GetTaxExcludedSum(Order order)
        {
            var plainSum = order.GetTotal();
            var services = CalculateServices(Calculations.Where(x => !x.IncludeTax), plainSum);
            var tax = TaxIncluded ? order.GetTotalTaxAmount(TaxIncluded, plainSum, services) : 0;
            return plainSum - tax;
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
            var result = Orders.Where(x => x.CalculatePrice).Sum(x => x.GetTotalTaxAmount(TaxIncluded, plainSum, preTaxServices));
            return result;
        }

        private decimal CalculateServices(IEnumerable<Calculation> calculations, decimal sum)
        {
            decimal totalAmount = 0;
            var currentSum = sum;

            foreach (var calculation in calculations.OrderBy(x => x.Order))
            {
                var sumValue = calculation.UsePlainSum ? Orders.Where(x => x.DecreaseInventory || x.IncreaseInventory).Sum(x => x.GetVisibleValue()) : sum;

                calculation.Update(sumValue, currentSum, LocalSettings.Decimals);

                totalAmount += calculation.CalculationAmount;
                currentSum += calculation.CalculationAmount;

                if (calculation.Amount == 0 && calculation.CalculationType != 5)
                {
                    Calculations.Remove(calculation);
                }

                calculation.UpdateCalculationTransaction(TransactionDocument, Math.Abs(calculation.CalculationAmount), ExchangeRate);
            }

            return decimal.Round(totalAmount, LocalSettings.Decimals);
        }

        public void AddCalculation(CalculationType calculationType, decimal amount)
        {
            var calculation = Calculations.SingleOrDefault(x => x.CalculationTypeId == calculationType.Id) ??
                    Calculations.SingleOrDefault(x => x.AccountTransactionTypeId == calculationType.AccountTransactionType.Id);
            if (calculation == null)
            {
                calculation = new Calculation
                        {
                            Amount = amount,
                            Name = calculationType.Name,
                            CalculationType = calculationType.CalculationMethod,
                            CalculationTypeId = calculationType.Id,
                            IncludeTax = calculationType.IncludeTax,
                            DecreaseAmount = calculationType.DecreaseAmount,
                            UsePlainSum = calculationType.UsePlainSum,
                            Order = calculationType.SortOrder,
                            AccountTransactionTypeId = calculationType.AccountTransactionType.Id
                        };
                Calculations.Add(calculation);
                TransactionDocument.AddSingletonTransaction(calculation.AccountTransactionTypeId, calculationType.AccountTransactionType, GetTicketAccounts());
            }
            else if (calculation.Amount == amount)
            {
                amount = 0;
            }
            else calculation.Amount = amount;
            calculation.Name = calculationType.Name;
            if (amount == 0 && calculation.CalculationType != 5)
            {
                Calculations.Remove(calculation);
                calculation.UpdateCalculationTransaction(TransactionDocument, 0, ExchangeRate);
            }
        }

        public decimal GetPlainSum()
        {
            return Orders.Sum(item => item.GetTotal());
        }

        private decimal GetPlainSum(int taxTempleteAccountTransactionTypeId)
        {
            return
            Orders.Where(
                x =>
                x.GetTaxValues().Any(y => y.TaxTempleteAccountTransactionTypeId == taxTempleteAccountTransactionTypeId))
                  .Sum(x => x.GetTotal());
        }

        public decimal GetPaymentAmount()
        {
            return Payments.Sum(x => x.Amount);
        }

        public decimal GetChangeAmount()
        {
            return ChangePayments.Sum(x => x.Amount);
        }

        public decimal GetRemainingAmount()
        {
            var sum = GetSum();
            var payment = GetPaymentAmount();
            var changePayment = GetChangeAmount();
            return decimal.Round(sum - payment + changePayment, LocalSettings.Decimals);
        }

        public string UserString
        {
            get { return Name; }
        }

        public bool CanSubmit
        {
            get { return !IsClosed; }
        }

        public bool IsTagged
        {
            get { return TicketTagValues.Any(x => !string.IsNullOrEmpty(x.TagValue)); }
        }

        public bool IsInState(string stateName, string state)
        {
            if (stateName == "*") return TicketStateValues.Any(x => x.State == state);
            if (string.IsNullOrEmpty(state)) return TicketStateValues.All(x => x.StateName != stateName);
            return TicketStateValues.Any(x => x.StateName == stateName && x.State == state);
        }

        public void CancelOrders(IEnumerable<Order> orders)
        {
            UnLock();
            foreach (var order in orders.Where(order => order.Id == 0))
            {
                RemoveOrder(order);
            }
        }

        public bool CanRemoveSelectedOrders(IEnumerable<Order> items)
        {
            return (items.Where(x => x.CalculatePrice).Sum(x => x.GetSelectedValue()) <= GetRemainingAmount());
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
                        x.PortionName == ti.PortionName && x.CalculatePrice == ti.CalculatePrice && x.Price == ti.Price);
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

            Orders.Where(x => x.Id == 0).ToList().ForEach(x => x.CreatedDateTime = DateTime.Now);
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
            if (_shouldLock) Lock();
            _shouldLock = false;
        }

        public static Ticket Create(Department department, TicketType ticketType, decimal exchangeRate, IEnumerable<CalculationType> calculationTypes)
        {
            var ticket = new Ticket
                {
                    TicketTypeId = ticketType.Id,
                    DepartmentId = department.Id,
                    TaxIncluded = ticketType.TaxIncluded,
                    TransactionDocument = new AccountTransactionDocument(),
                    ExchangeRate = exchangeRate
                };

            if (calculationTypes != null)
            {
                foreach (var calculationType in calculationTypes.OrderBy(x => x.SortOrder))
                {
                    ticket.AddCalculation(calculationType, calculationType.Amount);
                }
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

        public TicketStateValue GetStateValue(string groupName)
        {
            return TicketStateValues.SingleOrDefault(x => x.StateName == groupName) ?? TicketStateValue.Default;
        }

        public void SetStateValue(string stateName, string state, string stateValue, int quantity = 0)
        {
            var sv = TicketStateValues.SingleOrDefault(x => x.StateName == stateName);
            if (sv == null)
            {
                sv = new TicketStateValue { StateName = stateName, State = state, StateValue = stateValue, Quantity = quantity };
                TicketStateValues.Add(sv);
            }
            else
            {
                sv.State = state;
                sv.StateValue = stateValue;
                sv.Quantity = quantity;
            }
            if (string.IsNullOrEmpty(sv.State))
                TicketStateValues.Remove(sv);

            TicketStates = JsonHelper.Serialize(TicketStateValues);
            _ticketStateValues = null;
        }

        public string GetStateData()
        {
            return string.Join("\r", TicketStateValues.Where(x => !string.IsNullOrEmpty(x.State)).Select(x => string.Format("{0}{1}: {2} {3}", x.Quantity > 0 ? string.Format("{0} ", x.Quantity.ToString(CultureInfo.CurrentCulture)) : "", x.StateName, x.State, !string.IsNullOrEmpty(x.StateValue) ? string.Format("[{0}]", x.StateValue) : "")));
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

        public void UpdateEntity(int entityTypeId, int entityId, string entityName, int accountTypeId, int accountId, string entityCustomData)
        {
            var r = TicketEntities.SingleOrDefault(x => x.EntityTypeId == entityTypeId);
            if (r == null && entityId > 0)
            {
                TicketEntities.Add(new TicketEntity
                {
                    EntityId = entityId,
                    EntityName = entityName,
                    EntityTypeId = entityTypeId,
                    AccountTypeId = accountTypeId,
                    AccountId = accountId,
                    EntityCustomData = entityCustomData
                });
            }
            else if (r != null && entityId > 0)
            {
                r.AccountId = accountId;
                r.AccountTypeId = accountTypeId;
                r.EntityId = entityId;
                r.EntityName = entityName;
                r.EntityTypeId = entityTypeId;
            }
            else if (r != null && entityId == 0)
                TicketEntities.Remove(r);
        }

        public IList<int> GetTaxIds()
        {
            return Orders.SelectMany(x => x.TaxValues)
                               .Where(x => x.TaxTempleteAccountTransactionTypeId > 0)
                               .Select(x => x.TaxTempleteAccountTransactionTypeId)
                               .Distinct().ToList();
        }

        public void Recalculate()
        {
            if (Orders.Count > 0)
            {
                var orderGroup = Orders.GroupBy(x => x.AccountTransactionTypeId);
                foreach (var orders in orderGroup)
                {
                    var o = orders;
                    var transaction = TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == o.Key);
                    var amount = o.Sum(x => GetTaxExcludedSum(x));
                    transaction.UpdateAccounts(GetTicketAccounts());
                    transaction.UpdateAmount(amount, ExchangeRate);
                }

                var taxIds = GetTaxIds();
                if (taxIds.Any())
                {
                    var plainSum = GetPlainSum();
                    var preTaxServices = GetPreTaxServicesTotal();
                    foreach (var taxId in taxIds)
                    {
                        var transaction = TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == taxId);
                        transaction.UpdateAccounts(GetTicketAccounts());
                        transaction.UpdateAmount(GetTaxTotal(taxId, preTaxServices, plainSum), ExchangeRate);
                    }
                }
            }

            RemainingAmount = GetRemainingAmount();
            TotalAmount = GetSum();
        }

        public decimal GetTaxTotal(int taxTemplateAccountTransactionTypeId, decimal preTaxServicesTotal, decimal plainSum)
        {
            var result = Orders.Sum(x => x.GetTotalTaxAmount(TaxIncluded, plainSum, preTaxServicesTotal, taxTemplateAccountTransactionTypeId));
            return result;
        }

        public decimal GetTaxTotal()
        {
            var result = Orders.Sum(x => x.GetTotalTaxAmount(TaxIncluded, GetPlainSum(), GetPreTaxServicesTotal()));
            return result;
        }

        public IEnumerable<Order> SelectedOrders { get { return Orders.Where(x => x.IsSelected); } }

        public IEnumerable<Order> ExtractSelectedOrders()
        {
            if (Orders.Any(x => x.SelectedQuantity > 0 && x.SelectedQuantity < x.Quantity))
            {
                Orders.ToList().ForEach(x => x.IsSelected = false);
                var newOrders = FixSelectedOrders();
                newOrders.ToList().ForEach(x => x.IsSelected = true);
            }
            return SelectedOrders;
        }

        private IEnumerable<Order> FixSelectedOrders()
        {
            var newItems = new List<Order>();

            foreach (var order in Orders.Where(x => x.SelectedQuantity > 0 && x.SelectedQuantity < x.Quantity).ToList())
            {
                Debug.Assert(order.SelectedQuantity > 0);
                Debug.Assert(Orders.Contains(order));
                if (order.SelectedQuantity >= order.Quantity) continue;
                var newItem = CloneOrder(order);
                newItem.Id = 0;
                newItem.Quantity = order.SelectedQuantity;
                order.Quantity -= order.SelectedQuantity;
                order.ResetSelectedQuantity();
                newItems.Add(newItem);
            }
            return newItems;
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
            return (GetRemainingAmount() == 0 || TicketEntities.Count > 0 ||
                 IsTagged || Orders.Count == 0);
        }

        public decimal GetCalculationTotal(string s)
        {
            return Calculations.Where(x => string.IsNullOrEmpty(s) || x.Name == s).Sum(x => x.CalculationAmount);
        }

        public string GetEntityName(int entityTypeId)
        {
            var tr = TicketEntities.FirstOrDefault(x => x.EntityTypeId == entityTypeId);
            if (tr != null) return tr.EntityName;
            return "";
        }

        public decimal GetOrderStateTotal(string s)
        {
            return Orders.Where(x => x.IsInState("*", s)).Sum(x => x.GetValue());
        }

        public decimal GetActiveTimerAmount()
        {
            return Orders.Where(x => x.ProductTimerValue != null && x.ProductTimerValue.IsActive).Sum(x => x.GetValue());
        }

        public bool HasActiveTimers()
        {
            return Orders.Any(x => x.ProductTimerValue != null && x.ProductTimerValue.IsActive);
        }

        public void StopActiveTimers()
        {
            Orders.Where(x => x.ProductTimerValue != null && x.ProductTimerValue.IsActive).ToList().ForEach(x => x.StopProductTimer());
        }

        public IEnumerable<PaidItem> GetPaidItems()
        {
            return _paidItems;
        }

        public void RemoveZeroAmountAccountTransactions()
        {
            if (!IsClosed) return;
            TransactionDocument.AccountTransactions.Where(x => x.Amount == 0).ToList().ForEach(x => TransactionDocument.AccountTransactions.Remove(x));
        }

        public string GetStateStr(string s)
        {
            var sv = GetStateValue(s);
            return sv != null ? sv.State : "";
        }
    }
}
