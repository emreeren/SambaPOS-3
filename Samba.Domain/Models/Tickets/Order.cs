using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Diagnostics;
using Samba.Domain.Models.Menus;

namespace Samba.Domain.Models.Tickets
{
    public class Order
    {
        public Order()
        {
            _orderTagValues = new List<OrderTagValue>();
            CreatedDateTime = DateTime.Now;
            ModifiedDateTime = DateTime.Now;
            _selectedQuantity = 0;
        }

        public int Id { get; set; }
        public int TicketId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string PortionName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public int PortionCount { get; set; }
        public bool Locked { get; set; }
        public bool Voided { get; set; }
        public bool Gifted { get; set; }
        public int OrderNumber { get; set; }
        public int CreatingUserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int ModifiedUserId { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        [StringLength(10)]
        public string PriceTag { get; set; }
        public string Tag { get; set; }

        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public int TaxTemplateId { get; set; }
        public bool TaxIncluded { get; set; }

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

        public void UpdateMenuItem(int userId, MenuItem menuItem, string portionName, string priceTag, int quantity)
        {
            MenuItemId = menuItem.Id;
            MenuItemName = menuItem.Name;
            var portion = menuItem.GetPortion(portionName);
            Debug.Assert(portion != null);
            UpdatePortion(portion, priceTag, menuItem.TaxTemplate);
            Quantity = quantity;
            _selectedQuantity = quantity;
            PortionCount = menuItem.Portions.Count;
            CreatingUserId = userId;
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

        private void TagOrder(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId)
        {
            var otag = new OrderTagValue
                       {
                           Name = orderTag.Name,
                           OrderTagGroupId = orderTagGroup.Id,
                           MenuItemId = orderTag.MenuItemId,
                           AddTagPriceToOrderPrice = orderTagGroup.AddTagPriceToOrderPrice,
                           PortionName = PortionName,
                           TagAction = orderTagGroup.TagAction,
                           Quantity = 1
                       };
            otag.UpdatePrice(TaxIncluded, TaxRate, orderTag.Price);

            OrderTagValues.Add(otag);

            if (orderTagGroup.VoidsOrder) Void(userId);
            if (orderTagGroup.GiftsOrder) Gift(userId);
        }

        private void UntagOrder(OrderTagGroup orderTagGroup, OrderTagValue orderTagValue)
        {
            OrderTagValues.Remove(orderTagValue);
            if (orderTagGroup.GiftsOrder || orderTagGroup.VoidsOrder)
            {
                CancelGiftOrVoid();
            }
        }

        public void ToggleOrderTag(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId)
        {
            var otag = OrderTagValues.FirstOrDefault(x => x.Name == orderTag.Name);
            if (otag == null)
            {
                if (orderTagGroup.IsSingleSelection)
                    OrderTagValues.Where(x => x.OrderTagGroupId == orderTagGroup.Id).ToList().ForEach(x => OrderTagValues.Remove(x));
                TagOrder(orderTagGroup, orderTag, userId);
            }
            else if (orderTagGroup.IsQuantitySelection)
            {
                otag.Quantity++;
            }
            else
            {
                UntagOrder(orderTagGroup, otag);
            }
            if (orderTagGroup.UnlocksOrder) 
                Locked = false;
        }

        public OrderTagValue GetCustomOrderTag()
        {
            return OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == 0);
        }

        public OrderTagValue GetOrCreateCustomOrderTagValue()
        {
            var tip = GetCustomOrderTag();
            if (tip == null)
            {
                tip = new OrderTagValue
                          {
                              Name = "",
                              Price = 0,
                              OrderTagGroupId = 0,
                              MenuItemId = 0,
                              Quantity = 0
                          };
                OrderTagValues.Add(tip);
            }
            return tip;
        }

        public void UpdateCustomOrderTag(string text, decimal price, decimal quantity)
        {
            var orderTag = GetOrCreateCustomOrderTagValue();
            if (string.IsNullOrEmpty(text))
            {
                OrderTagValues.Remove(orderTag);
            }
            else
            {
                orderTag.Name = text;
                orderTag.Quantity = quantity;
                orderTag.Price = price;
                if (TaxIncluded && TaxRate > 0)
                {
                    orderTag.Price = orderTag.Price / ((100 + TaxRate) / 100);
                    orderTag.Price = decimal.Round(orderTag.Price, 2);
                    orderTag.TaxAmount = price - orderTag.Price;
                }
                else if (TaxRate > 0) orderTag.TaxAmount = (price * TaxRate) / 100;
                else TaxAmount = 0;
            }
        }

        public decimal GetTotal()
        {
            return Voided || Gifted ? 0 : GetItemValue();
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
            TaxIncluded = taxTemplate.TaxIncluded;
            UpdatePrice(Price, PriceTag);
        }

        private void Void(int userId)
        {
            if (Locked && !Voided && !Gifted)
            {
                Voided = true;
                ModifiedUserId = userId;
                ModifiedDateTime = DateTime.Now;
                Locked = false;
            }
            else CancelGiftOrVoid();
        }

        private void Gift(int userId)
        {
            Gifted = true;
            ModifiedUserId = userId;
            ModifiedDateTime = DateTime.Now;
        }

        public void CancelGiftOrVoid()
        {
            if (Voided && !Locked)
            {
                Voided = false;
                OrderTagValues.Where(x => x.VoidsOrder).ToList().ForEach(x => OrderTagValues.Remove(x));
            }
            else if (Gifted)
            {
                Gifted = false;
                OrderTagValues.Where(x => x.GiftsOrder).ToList().ForEach(x => OrderTagValues.Remove(x));
            }
            if (Id > 0) Locked = true;
        }
    }
}
