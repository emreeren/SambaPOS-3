using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Diagnostics;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class Order : Value
    {
        public Order()
        {
            _selectedQuantity = 0;
            _orderTagValues = new List<OrderTagValue>();
            CreatedDateTime = DateTime.Now;
            CalculatePrice = true;
            DecreaseInventory = true;
        }

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
        public int AccountTransactionTemplateId { get; set; }
        public int? MenuItemTimerValueId { get; set; }
        public virtual MenuItemTimerValue MenuItemTimerValue { get; set; }

        [StringLength(10)]
        public string PriceTag { get; set; }
        public string Tag { get; set; }

        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxTemplateName { get; set; }
        public int TaxTemplateId { get; set; }
        public bool TaxIncluded { get; set; }
        public int TaxTempleteAccountTransactionTemplateId { get; set; }

        private IList<OrderTagValue> _orderTagValues;
        public virtual IList<OrderTagValue> OrderTagValues
        {
            get { return _orderTagValues; }
            set { _orderTagValues = value; }
        }

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

        private static Order _null;
        public static Order Null { get { return _null ?? (_null = new Order { MenuItemTimerValue = new MenuItemTimerValue() }); } }

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
                TaxTempleteAccountTransactionTemplateId = taxTemplate.AccountTransactionTemplate.Id;
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

            foreach (var orderTagValue in OrderTagValues)
            {
                orderTagValue.PortionName = portion.Name;
            }
        }

        public void TagIfNotTagged(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId)
        {
            if (OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == orderTagGroup.Id && x.Name == orderTag.Name) == null)
            {
                ToggleOrderTag(orderTagGroup, orderTag, userId);
            }
        }

        public void UntagIfTagged(OrderTagGroup orderTagGroup, OrderTag orderTag)
        {
            var value = OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == orderTagGroup.Id && x.Name == orderTag.Name);
            if (value != null) UntagOrder(value);
        }

        private void TagOrder(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId, int tagIndex)
        {
            var otag = new OrderTagValue
                       {
                           Name = orderTag.Name,
                           OrderTagGroupId = orderTagGroup.Id,
                           OrderTagGroupName = orderTagGroup.Name,
                           MenuItemId = orderTag.MenuItemId,
                           AddTagPriceToOrderPrice = orderTagGroup.AddTagPriceToOrderPrice,
                           PortionName = PortionName,
                           SubValue = !string.IsNullOrEmpty(orderTagGroup.ButtonHeader) && orderTag.Price == 0 && orderTagGroup.MaxSelectedItems == 1,
                           UserId = userId,
                           Quantity = 1,
                           NewTag = true,
                           OrderKey = orderTagGroup.Order.ToString("000") + orderTag.Order.ToString("000")
                       };

            otag.UpdatePrice(TaxIncluded, TaxRate, orderTag.Price);

            if (tagIndex > -1)
                OrderTagValues.Insert(tagIndex, otag);
            else
                OrderTagValues.Add(otag);
        }

        private void UntagOrder(OrderTagValue orderTagValue)
        {
            OrderTagValues.Remove(orderTagValue);
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
                if (orderStateGroup.UnlocksOrder && Id > 0) Locked = true;
                return;
            }
            CalculatePrice = orderStateGroup.CalculateOrderPrice;
            DecreaseInventory = orderStateGroup.DecreaseOrderInventory;
            IncreaseInventory = orderStateGroup.IncreaseOrderInventory;
            if (orderStateGroup.UnlocksOrder) Locked = false;
            if (orderStateGroup.AccountTransactionTemplateId > 0)
                AccountTransactionTemplateId = orderStateGroup.AccountTransactionTemplateId;
            OrderState = orderState.Name;
            OrderStateGroupName = orderStateGroup.Name;
            OrderStateGroupId = orderStateGroup.Id;
        }

        public bool ToggleOrderTag(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId)
        {
            var result = true;
            var otag = OrderTagValues.FirstOrDefault(x => x.Name == orderTag.Name);
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
                TagOrder(orderTagGroup, orderTag, userId, tagIndex);
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

        public decimal GetTransactionTotal()
        {
            if (CalculatePrice)
            {
                var tax = TaxIncluded ? GetTotalTaxAmount() : 0;
                return GetItemValue() - tax;
            }
            return 0;
        }

        public decimal GetTotal()
        {
            var val = GetTransactionTotal();
            if (IncreaseInventory) val = 0 - val;
            return val;
        }

        public decimal GetItemValue()
        {
            return Quantity * GetItemPrice();
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
            if (MenuItemTimerValue != null)
                result = MenuItemTimerValue.GetPrice(result);
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
            return OrderTagValues.Any(x => x.Name == model.Name);
        }

        public bool IsTaggedWith(OrderTagGroup orderTagGroup)
        {
            return OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == orderTagGroup.Id) != null;
        }

        public bool IsStateApplied(OrderStateGroup orderStateGroup)
        {
            return OrderStateGroupId == orderStateGroup.Id;
        }

        public decimal GetTotalTaxAmount()
        {
            var result = CalculatePrice && (DecreaseInventory || IncreaseInventory) ? (TaxAmount + OrderTagValues.Sum(x => x.TaxAmount)) * Quantity : 0;
            if (IncreaseInventory) result = 0 - result;
            return result;
        }

        public OrderTagValue GetOrderTagValue(string s)
        {
            if (OrderTagValues.Any(x => x.OrderTagGroupName == s))
                return OrderTagValues.First(x => x.OrderTagGroupName == s);
            return OrderTagValue.Empty;
        }

        public string SubOrderTags
        {
            get
            {
                return string.Join(", ", OrderTagValues.Where(x => x.SubValue).OrderBy(x => x.OrderKey).Select(x => x.ShortName));
            }
        }

        public string OrderKey { get { return string.Join("", OrderTagValues.OrderBy(x => x.OrderKey).Select(x => x.OrderKey)); } }

        public void UpdateProductTimer(MenuItemTimer timer)
        {
            if (timer != null)
            {
                MenuItemTimerValue = new MenuItemTimerValue
                                         {
                                             MenuItemTimerId = timer.Id,
                                             MinTime = timer.MinTime,
                                             PriceType = timer.PriceType,
                                             PriceDuration = timer.PriceDuration,
                                             TimeRounding = timer.TimeRounding,
                                         };
            }
        }

        public void StopMenuItemTimer()
        {
            if (MenuItemTimerValue != null)
                MenuItemTimerValue.Stop();
        }
    }
}
