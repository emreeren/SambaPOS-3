using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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
        public int OrderStateGroupId { get; set; }
        public string OrderStateGroupName { get; set; }
        public string OrderState { get; set; }
        public int OrderStateUserId { get; set; }
        public int OrderNumber { get; set; }
        public string CreatingUserName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int AccountTransactionTypeId { get; set; }
        public int? ProductTimerValueId { get; set; }
        public virtual ProductTimerValue ProductTimerValue { get; set; }

        public string PriceTag { get; set; }
        public string Tag { get; set; }

        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxTemplateName { get; set; }
        public int TaxTemplateId { get; set; }
        public bool TaxIncluded { get; set; }
        public int TaxTempleteAccountTransactionTypeId { get; set; }
        public string OrderTags { get; set; }

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

        public void UpdateMenuItem(string userName, MenuItem menuItem, MenuItemPortion portion, string priceTag, int quantity)
        {
            MenuItemId = menuItem.Id;
            MenuItemName = menuItem.Name;
            Debug.Assert(portion != null);
            UpdatePortion(portion, priceTag, menuItem.TaxTemplate);
            Quantity = quantity;
            _selectedQuantity = quantity;
            PortionCount = menuItem.Portions.Count;
            CreatingUserName = userName;
            CreatedDateTime = DateTime.Now;
        }

        public void UpdatePortion(MenuItemPortion portion, string priceTag, TaxTemplate taxTemplate)
        {
            PortionName = portion.Name;

            if (taxTemplate != null)
            {
                TaxRate = taxTemplate.Rate;
                TaxIncluded = taxTemplate.TaxIncluded;
                TaxTemplateId = taxTemplate.Id;
                TaxTemplateName = taxTemplate.Name;
                TaxTempleteAccountTransactionTypeId = taxTemplate.AccountTransactionType.Id;
            }

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
                           IsSubTag = !string.IsNullOrEmpty(orderTagGroup.ButtonHeader) && orderTag.Price == 0 && orderTagGroup.MaxSelectedItems == 1,
                           UserId = userId,
                           Quantity = 1,
                           OrderKey = orderTagGroup.Order.ToString("000") + orderTag.Order.ToString("000")
                       };

            otag.UpdatePrice(TaxIncluded, TaxRate, orderTag.Price);

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

        public void UpdateOrderState(OrderStateGroup orderStateGroup, OrderState orderState, int userId)
        {
            if (orderStateGroup.Id == OrderStateGroupId && orderState.Name == OrderState && Locked && orderStateGroup.UnlocksOrder) return;
            if (orderState == null || (orderStateGroup.Id == OrderStateGroupId && orderState.Name == OrderState))
            {
                CalculatePrice = true;
                DecreaseInventory = true;
                IncreaseInventory = false;
                OrderState = "";
                OrderStateGroupName = "";
                OrderStateGroupId = 0;
                if (Quantity < 0) Quantity = 0 - Quantity;
                if (orderStateGroup.UnlocksOrder && Id > 0) Locked = true;
                return;
            }
            CalculatePrice = orderStateGroup.CalculateOrderPrice;
            DecreaseInventory = orderStateGroup.DecreaseOrderInventory;
            IncreaseInventory = orderStateGroup.IncreaseOrderInventory;
            if (orderStateGroup.UnlocksOrder) Locked = false;
            if (orderStateGroup.AccountTransactionTypeId > 0)
                AccountTransactionTypeId = orderStateGroup.AccountTransactionTypeId;
            if (IncreaseInventory && Quantity > 0) Quantity = 0 - Quantity;
            if (!IncreaseInventory && Quantity < 0) Quantity = 0 - Quantity;
            OrderState = orderState.Name;
            OrderStateGroupName = orderStateGroup.Name;
            OrderStateGroupId = orderStateGroup.Id;
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

        public decimal GetTotal()
        {
            if (CalculatePrice)
            {
                var tax = TaxIncluded ? GetTaxAmount() : 0;
                return GetItemValue() - tax;
            }
            return 0;
        }

        public decimal GetItemValue()
        {
            return decimal.Round(Quantity * GetItemPrice(), 2, MidpointRounding.AwayFromZero);
        }

        public decimal GetSelectedValue()
        {
            return SelectedQuantity > 0 ? SelectedQuantity * GetItemPrice() : GetItemValue();
        }

        public decimal GetItemPrice()
        {
            return GetPlainPrice() + GetTotalOrderTagPrice();
        }

        public decimal GetPlainPrice()
        {
            var result = Price;
            if (TaxIncluded) result += TaxAmount;
            if (ProductTimerValue != null)
                result = ProductTimerValue.GetPrice(result);
            return result;
        }

        public decimal GetTotalOrderTagPrice()
        {
            return GetOrderTagSum(OrderTagValues, TaxIncluded);
        }

        public decimal GetOrderTagPrice()
        {
            return GetOrderTagSum(OrderTagValues.Where(x => !x.AddTagPriceToOrderPrice), TaxIncluded);
        }

        public decimal GetMenuItemOrderTagPrice()
        {
            return GetOrderTagSum(OrderTagValues.Where(x => x.AddTagPriceToOrderPrice), TaxIncluded);
        }

        private static decimal GetOrderTagSum(IEnumerable<OrderTagValue> orderTags, bool vatIncluded)
        {
            return orderTags.Sum(orderTag => (orderTag.Price + (vatIncluded ? orderTag.TaxAmount : 0)) * orderTag.Quantity);
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
            if (TaxIncluded && TaxRate > 0)
            {
                Price = Price / ((100 + TaxRate) / 100);
                Price = decimal.Round(Price, 2);
                TaxAmount = value - Price;
            }
            else if (TaxRate > 0) TaxAmount = (Price * TaxRate) / 100;
            else TaxAmount = 0;
        }

        public void UpdateTaxTemplate(TaxTemplate taxTemplate)
        {
            TaxRate = taxTemplate.Rate;
            TaxTemplateId = taxTemplate.Id;
            TaxTemplateName = taxTemplate.Name;
            TaxIncluded = taxTemplate.TaxIncluded;
            UpdatePrice(Price, PriceTag);
        }

        public bool IsTaggedWith(OrderTag model)
        {
            return OrderTagValues.Any(x => x.TagValue == model.Name);
        }

        public bool IsTaggedWith(OrderTagGroup orderTagGroup)
        {
            return OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == orderTagGroup.Id) != null;
        }

        public bool IsStateApplied(OrderStateGroup orderStateGroup)
        {
            return OrderStateGroupId == orderStateGroup.Id;
        }

        public decimal GetTaxAmount()
        {
            var result = CalculatePrice && (DecreaseInventory || IncreaseInventory) ? (TaxAmount + OrderTagValues.Sum(x => x.TaxAmount)) * Quantity : 0;
            return result;
        }

        public decimal GetTotalTaxAmount(decimal plainSum, decimal preTaxServices)
        {
            var result = CalculatePrice && (DecreaseInventory || IncreaseInventory) ? (TaxAmount + OrderTagValues.Sum(x => x.TaxAmount)) * Quantity : 0;
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

        public string SubOrderTags
        {
            get
            {
                return string.Join(", ", OrderTagValues.Where(x => x.IsSubTag).OrderBy(x => x.OrderKey).Select(x => x.ShortName));
            }
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


    }
}
