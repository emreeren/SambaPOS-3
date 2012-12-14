using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Tickets
{
    public class Order : Value
    {
        public Order()
        {
            _selectedQuantity = 0;
            CreatedDateTime = DateTime.Now;
            CalculatePrice = true;
            DecreaseInventory = true;
        }

        public bool IsSelected { get; set; } // Not Stored

        public int TicketId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string PortionName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public int PortionCount { get; set; }
        public bool Locked { get; set; }
        public bool CalculatePrice { get; set; }
        public bool DecreaseInventory { get; set; }
        public bool IncreaseInventory { get; set; }
        public int OrderNumber { get; set; }
        public string CreatingUserName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int AccountTransactionTypeId { get; set; }
        public int? ProductTimerValueId { get; set; }
        public virtual ProductTimerValue ProductTimerValue { get; set; }

        public string PriceTag { get; set; }
        public string Tag { get; set; }

        public string Taxes { get; set; }
        public string OrderTags { get; set; }
        public string OrderStates { get; set; }

        private decimal _selectedQuantity;
        public decimal SelectedQuantity
        {
            get { return _selectedQuantity; }
        }

        public string Description
        {
            get
            {
                var desc = MenuItemName + GetPortionDesc();
                if (SelectedQuantity > 0 && SelectedQuantity != Quantity)
                    desc = string.Format("({0:#.##}) {1}", SelectedQuantity, desc);
                return desc;
            }
        }

        private IList<OrderTagValue> _orderTagValues;
        internal IList<OrderTagValue> OrderTagValues
        {
            get { return _orderTagValues ?? (_orderTagValues = JsonHelper.Deserialize<List<OrderTagValue>>(OrderTags)); }
        }


        private IList<OrderStateValue> _orderStateValues;
        internal IList<OrderStateValue> OrderStateValues
        {
            get { return _orderStateValues ?? (_orderStateValues = JsonHelper.Deserialize<List<OrderStateValue>>(OrderStates)); }
        }

        private IList<TaxValue> _taxValues;
        internal IList<TaxValue> TaxValues
        {
            get { return _taxValues ?? (_taxValues = JsonHelper.Deserialize<List<TaxValue>>(Taxes)); }
        }

        public bool OrderTagExists(Func<OrderTagValue, bool> prediction)
        {
            return OrderTagValues.Any(prediction);
        }

        public IEnumerable<OrderTagValue> GetOrderTagValues(Func<OrderTagValue, bool> prediction)
        {
            return OrderTagValues.Where(prediction);
        }

        public IEnumerable<OrderTagValue> GetOrderTagValues()
        {
            return OrderTagValues;
        }

        private static Order _null;
        public static Order Null { get { return _null ?? (_null = new Order { ProductTimerValue = new ProductTimerValue() }); } }

        public void UpdateMenuItem(string userName, MenuItem menuItem, IEnumerable<TaxTemplate> taxTemplates, MenuItemPortion portion, string priceTag, int quantity)
        {
            MenuItemId = menuItem.Id;
            MenuItemName = menuItem.Name;
            Debug.Assert(portion != null);
            UpdatePortion(portion, priceTag, taxTemplates);
            Quantity = quantity;
            _selectedQuantity = quantity;
            PortionCount = menuItem.Portions.Count;
            CreatingUserName = userName;
            CreatedDateTime = DateTime.Now;
        }

        public void UpdatePortion(MenuItemPortion portion, string priceTag, IEnumerable<TaxTemplate> taxTemplates)
        {
            PortionName = portion.Name;

            UpdateTaxTemplates(taxTemplates);

            if (!string.IsNullOrEmpty(priceTag))
            {
                string tag = priceTag;
                var price = portion.Prices.SingleOrDefault(x => x.PriceTag == tag);
                if (price != null && price.Price > 0)
                {
                    UpdatePrice(price.Price, price.PriceTag);
                }
                else priceTag = "";
            }

            if (string.IsNullOrEmpty(priceTag))
            {
                UpdatePrice(portion.Price, "");
            }

            if (OrderTagValues.Any(x => x.MenuItemId > 0 && x.PortionName != portion.Name))
            {
                foreach (var orderTagValue in OrderTagValues.Where(x => x.MenuItemId > 0))
                {
                    orderTagValue.PortionName = portion.Name;
                }
                OrderTags = JsonHelper.Serialize(OrderTagValues);
                _orderTagValues = null;
            }
        }

        public void TagIfNotTagged(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId, string tagNote)
        {
            if (OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == orderTagGroup.Id && x.TagValue == orderTag.Name) == null)
            {
                ToggleOrderTag(orderTagGroup, orderTag, userId, tagNote);
            }
        }

        public bool UntagIfTagged(OrderTagGroup orderTagGroup, OrderTag orderTag)
        {
            var value = OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == orderTagGroup.Id && x.TagValue == orderTag.Name);
            if (value != null)
            {
                UntagOrder(value);
                return true;
            }
            return false;
        }

        private void TagOrder(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId, int tagIndex, string tagNote)
        {
            var otag = new OrderTagValue
                       {
                           TagValue = orderTag.Name,
                           OrderTagGroupId = orderTagGroup.Id,
                           TagName = orderTagGroup.Name,
                           TagNote = !string.IsNullOrEmpty(tagNote) ? tagNote : null,
                           MenuItemId = orderTag.MenuItemId,
                           AddTagPriceToOrderPrice = orderTagGroup.AddTagPriceToOrderPrice,
                           PortionName = orderTag.MenuItemId > 0 ? PortionName : null,
                           UserId = userId,
                           Quantity = 1,
                           OrderKey = orderTagGroup.SortOrder.ToString("000") + orderTag.SortOrder.ToString("000")
                       };

            otag.UpdatePrice(orderTag.Price);

            if (tagIndex > -1)
                OrderTagValues.Insert(tagIndex, otag);
            else
                OrderTagValues.Add(otag);
            OrderTags = JsonHelper.Serialize(OrderTagValues);
            _orderTagValues = null;
        }

        public void UntagOrder(OrderTagValue orderTagValue)
        {
            OrderTagValues.Remove(orderTagValue);
            OrderTags = JsonHelper.Serialize(OrderTagValues);
            _orderTagValues = null;
        }

        public bool ToggleOrderTag(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId, string tagNote)
        {
            var result = true;
            var otag = OrderTagValues.FirstOrDefault(x => x.TagValue == orderTag.Name);
            if (otag == null)
            {
                if (orderTagGroup.MaxSelectedItems > 1 && OrderTagValues.Count(x => x.OrderTagGroupId == orderTagGroup.Id) >= orderTagGroup.MaxSelectedItems) return false;
                var tagIndex = -1;
                if (orderTagGroup.MaxSelectedItems == 1)
                {
                    var sTag = OrderTagValues.SingleOrDefault(x => x.OrderTagGroupId == orderTag.OrderTagGroupId);
                    if (sTag != null) tagIndex = OrderTagValues.IndexOf(sTag);
                    OrderTagValues.Where(x => x.OrderTagGroupId == orderTagGroup.Id).ToList().ForEach(x => OrderTagValues.Remove(x));
                }
                TagOrder(orderTagGroup, orderTag, userId, tagIndex, tagNote);
            }
            else
            {
                otag.Quantity++;
                if (orderTagGroup.MaxSelectedItems == 1 || (orderTag.MaxQuantity > 0 && otag.Quantity > orderTag.MaxQuantity))
                {
                    UntagOrder(otag);
                    result = false;
                }
            }
            return result;
        }

        public OrderStateValue GetStateValue(string groupName)
        {
            return OrderStateValues.SingleOrDefault(x => x.StateName == groupName) ?? OrderStateValue.Default;
        }

        public void SetStateValue(string groupName, int groupOrder, string state, int stateOrder, string stateValue)
        {
            var sv = OrderStateValues.SingleOrDefault(x => x.StateName == groupName);
            if (sv == null)
            {
                sv = new OrderStateValue { StateName = groupName, State = state, StateValue = stateValue };
                OrderStateValues.Add(sv);
            }
            else
            {
                sv.State = state;
                sv.StateValue = stateValue;
            }

            sv.OrderKey = groupOrder.ToString("000") + stateOrder.ToString("000");

            if (string.IsNullOrEmpty(sv.State))
                OrderStateValues.Remove(sv);

            OrderStates = JsonHelper.Serialize(OrderStateValues);
            _orderStateValues = null;
        }

        public string GetStateDesc()
        {
            return string.Join(",",
                           OrderStateValues.OrderBy(x => x.OrderKey).Where(x => !string.IsNullOrEmpty(x.State)).Select(
                               x =>
                               string.Format("{0} {1}", x.State.Trim(),
                                             !string.IsNullOrEmpty(x.StateValue)
                                                 ? string.Format(":{0}", x.StateValue.Trim())
                                                 : "")));
        }

        public string GetStateData()
        {
            return string.Join("\r", OrderStateValues.Where(x => !string.IsNullOrEmpty(x.State)).OrderBy(x => x.OrderKey).Select(x => string.Format("{0} {1}", x.State, !string.IsNullOrEmpty(x.StateValue) ? string.Format("[{0}]", x.StateValue) : "")));
        }

        public decimal GetOrderTagPrice()
        {
            return GetOrderTagSum(OrderTagValues.Where(x => !x.AddTagPriceToOrderPrice));
        }

        private static decimal GetOrderTagSum(IEnumerable<OrderTagValue> orderTags)
        {
            return orderTags.Sum(orderTag => orderTag.Price * orderTag.Quantity);
        }

        public void IncSelectedQuantity()
        {
            _selectedQuantity++;
            if (SelectedQuantity > Quantity) _selectedQuantity = 1;
        }

        public void DecSelectedQuantity()
        {
            _selectedQuantity--;
            if (SelectedQuantity < 1) _selectedQuantity = 1;
        }

        public void ResetSelectedQuantity()
        {
            _selectedQuantity = Quantity;
        }

        public string GetPortionDesc()
        {
            if (PortionCount > 1
                && !string.IsNullOrEmpty(PortionName)
                && !string.IsNullOrEmpty(PortionName.Trim('\b', ' ', '\t'))
                && PortionName.ToLower() != "normal")
                return "." + PortionName;
            return "";
        }

        public void UpdatePrice(decimal value, string priceTag)
        {
            Price = value;
            PriceTag = priceTag;
            var newPrice = GetPrice();
            TaxValues.ToList().ForEach(x => x.UpdateTaxAmount(newPrice, TaxValues.Sum(y => y.TaxRate)));
        }

        public void UpdateTaxTemplates(IEnumerable<TaxTemplate> taxTemplates)
        {
            if (taxTemplates == null) return;
            TaxValues.Clear();
            foreach (var template in taxTemplates)
            {
                TaxValues.Add(new TaxValue(template));
            }
            Taxes = JsonHelper.Serialize(TaxValues);
            _taxValues = null;
        }

        public void ExcludeTax()
        {
            foreach (var taxValue in TaxValues)
            {
                taxValue.TaxIncluded = false;
            }
            Taxes = JsonHelper.Serialize(TaxValues);
            _taxValues = null;
        }

        public bool IsTaggedWith(OrderTag model)
        {
            return OrderTagValues.Any(x => x.TagValue == model.Name);
        }

        public bool IsTaggedWith(OrderTagGroup orderTagGroup)
        {
            return OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == orderTagGroup.Id) != null;
        }

        public decimal GetTotalTaxAmount(decimal plainSum, decimal preTaxServices)
        {
            var result = CalculatePrice ? TaxValues.Sum(x => x.TaxAmount) * Quantity : 0;
            if (preTaxServices != 0)
                result += (result * preTaxServices) / plainSum;
            return result;
        }

        public OrderTagValue GetOrderTagValue(string s)
        {
            if (OrderTagValues.Any(x => x.TagName == s))
                return OrderTagValues.First(x => x.TagName == s);
            return OrderTagValue.Empty;
        }

        public string OrderKey { get { return string.Join("", OrderTagValues.OrderBy(x => x.OrderKey).Select(x => x.OrderKey)); } }

        public void UpdateProductTimer(ProductTimer timer)
        {
            if (timer != null)
            {
                ProductTimerValue = new ProductTimerValue
                                         {
                                             ProductTimerId = timer.Id,
                                             MinTime = timer.MinTime,
                                             PriceType = timer.PriceType,
                                             PriceDuration = timer.PriceDuration,
                                             TimeRounding = timer.TimeRounding,
                                         };
            }
        }

        public void StopProductTimer()
        {
            if (ProductTimerValue != null)
                ProductTimerValue.Stop();
        }


        public bool IsInState(string stateName, string state)
        {
            if (stateName == "*") return OrderStateValues.Any(x => x.State == state);
            if (string.IsNullOrEmpty(state)) return OrderStateValues.All(x => x.StateName != stateName);
            return OrderStateValues.Any(x => x.StateName == stateName && x.State == state);
        }

        public bool IsInState(string stateValue)
        {
            return OrderStateValues.Any(x => x.StateValue == stateValue);
        }

        public IEnumerable<OrderStateValue> GetOrderStateValues()
        {
            return OrderStateValues;
        }

        public decimal GetTax()
        {
            return
                TaxValues.Where(x => !x.TaxIncluded).Sum(x => x.TaxAmount) -
                TaxValues.Where(x => x.TaxIncluded).Sum(x => x.TaxAmount);
        }

        // Vergi etkilemiş fiyat
        public decimal GetFinalPrice()
        {
            var result = GetPrice();
            result += GetTax();
            return result;
        }

        //Vergi etkilememiş fiyat
        public decimal GetPrice()
        {
            var result = Price + OrderTagValues.Sum(x => x.Price * x.Quantity);
            if (ProductTimerValue != null)
                result = ProductTimerValue.GetPrice(result);
            return result;
        }

        //Görünen fiyat
        public decimal GetVisiblePrice()
        {
            var result = Price + OrderTagValues.Where(x => x.AddTagPriceToOrderPrice).Sum(x => x.Price * x.Quantity);
            if (ProductTimerValue != null)
                result = ProductTimerValue.GetPrice(result + GetOrderTagPrice());
            return result;
        }

        public decimal GetVisibleValue()
        {
            return GetVisiblePrice() * Quantity;
        }

        public decimal GetFinalValue()
        {
            return decimal.Round(GetFinalPrice() * Quantity, 2, MidpointRounding.AwayFromZero);
        }

        public decimal GetSelectedValue()
        {
            return SelectedQuantity > 0 ? SelectedQuantity * GetFinalPrice() : GetFinalValue();
        }

        public decimal GetTotalTax()
        {
            return TaxValues.Sum(x => x.TaxAmount) * Quantity;
        }

        public decimal GetTotal()
        {
            if (CalculatePrice)
            {
                return GetFinalValue();
            }
            return 0;
        }
    }

    [DataContract]
    internal class TaxValue
    {
        public TaxValue()
        {

        }

        public TaxValue(TaxTemplate taxTemplate)
        {
            TaxRate = taxTemplate.Rate;
            TaxTemplateId = taxTemplate.Id;
            TaxTemplateName = taxTemplate.Name;
            TaxIncluded = taxTemplate.TaxIncluded;
            TaxTempleteAccountTransactionTypeId = taxTemplate.AccountTransactionType.Id;
        }

        [DataMember(Name = "TR")]
        public decimal TaxRate { get; set; }
        [DataMember(Name = "TA")]
        public decimal TaxAmount { get; set; }
        [DataMember(Name = "TN")]
        public string TaxTemplateName { get; set; }
        [DataMember(Name = "TT")]
        public int TaxTemplateId { get; set; }
        [DataMember(Name = "TI")]
        public bool TaxIncluded { get; set; }
        [DataMember(Name = "AT")]
        public int TaxTempleteAccountTransactionTypeId { get; set; }

        public void UpdateTaxAmount(decimal price, decimal totalRate)
        {
            if (TaxIncluded && totalRate > 0)
            {
                TaxAmount = decimal.Round((price * TaxRate) / (100 + totalRate), 2, MidpointRounding.AwayFromZero);
            }
            else if (TaxRate > 0) TaxAmount = (price * TaxRate) / 100;
            else TaxAmount = 0;
        }

        public decimal GetTaxAmount(decimal plainSum, decimal preTaxServices)
        {
            var result = TaxAmount;
            if (preTaxServices != 0)
                result += (result * preTaxServices) / plainSum;
            return result;
        }
    }
}
