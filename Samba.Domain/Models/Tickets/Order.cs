using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Diagnostics;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Settings;

namespace Samba.Domain.Models.Tickets
{
    public class Order
    {
        public Order()
        {
            _orderTagValues = new List<OrderTagValue>();
            CreatedDateTime = DateTime.Now;
            ModifiedDateTime = DateTime.Now;
            SelectedQuantity = 0;
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
        public int ReasonId { get; set; }
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

        public decimal SelectedQuantity { get; private set; }

        public void UpdateMenuItem(int userId, MenuItem menuItem, string portionName, string priceTag, int quantity)
        {
            MenuItemId = menuItem.Id;
            MenuItemName = menuItem.Name;
            var portion = menuItem.GetPortion(portionName);
            Debug.Assert(portion != null);
            UpdatePortion(portion, priceTag, menuItem.TaxTemplate);
            Quantity = quantity;
            SelectedQuantity = quantity;
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

        public void ToggleOrderTag(OrderTagGroup tagGroup, OrderTag orderTag)
        {
            if (tagGroup.MultipleSelection && orderTag.Price == 0)
            {
                var groupItems = OrderTagValues.Where(x => x.OrderTagGroupId == tagGroup.Id).ToList();
                foreach (var tip in groupItems) OrderTagValues.Remove(tip);
                Quantity = 1;
                return;
            }

            var ti = FindOrderTag(orderTag.Name);
            if (ti == null)
            {
                ti = new OrderTagValue
                        {
                            Name = orderTag.Name,
                            Price = orderTag.Price,
                            OrderTagGroupId = tagGroup.Id,
                            MenuItemId = orderTag.MenuItemId,
                            CalculateWithParentPrice = tagGroup.CalculateWithParentPrice,
                            PortionName = PortionName,
                            Quantity = tagGroup.MultipleSelection ? 0 : 1
                        };

                if (TaxIncluded && TaxRate > 0)
                {
                    ti.Price = ti.Price / ((100 + TaxRate) / 100);
                    ti.Price = decimal.Round(ti.Price, 2);
                    ti.TaxAmount = orderTag.Price - ti.Price;
                }
                else if (TaxRate > 0) ti.TaxAmount = (orderTag.Price * TaxRate) / 100;
                else ti.TaxAmount = 0;
            }
            if (tagGroup.SingleSelection)
            {
                var tip = OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == tagGroup.Id);
                if (tip != null)
                {
                    OrderTagValues.Insert(OrderTagValues.IndexOf(tip), ti);
                    OrderTagValues.Remove(tip);
                }
            }
            else if (tagGroup.MultipleSelection)
            {
                ti.Quantity++;
            }
            else if (!tagGroup.MultipleSelection && OrderTagValues.Contains(ti))
            {
                OrderTagValues.Remove(ti);
                return;
            }

            if (!OrderTagValues.Contains(ti)) OrderTagValues.Add(ti);
        }

        public OrderTagValue GetCustomOrderTag()
        {
            return OrderTagValues.FirstOrDefault(x => x.OrderTagGroupId == 0);
        }

        public OrderTagValue GetOrCreateCustomProperty()
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
            var tip = GetOrCreateCustomProperty();
            if (string.IsNullOrEmpty(text))
            {
                OrderTagValues.Remove(tip);
            }
            else
            {
                tip.Name = text;
                tip.Price = price;
                if (TaxIncluded && TaxRate > 0)
                {
                    tip.Price = tip.Price / ((100 + TaxRate) / 100);
                    tip.Price = decimal.Round(tip.Price, 2);
                    tip.TaxAmount = price - tip.Price;
                }
                else if (TaxRate > 0) tip.TaxAmount = (price * TaxRate) / 100;
                else TaxAmount = 0;

                tip.Quantity = quantity;
            }
        }

        private OrderTagValue FindOrderTag(string orderTagName)
        {
            return OrderTagValues.FirstOrDefault(x => x.Name == orderTagName);
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
            var result = Price + GetTotalOrderTagPrice();
            if (TaxIncluded) result += TaxAmount;
            return result;
        }

        public decimal GetTotalOrderTagPrice()
        {
            return GetOrderTagSum(OrderTagValues, TaxIncluded);
        }

        public decimal GetOrderTagPrice()
        {
            return GetOrderTagSum(OrderTagValues.Where(x => !x.CalculateWithParentPrice), TaxIncluded);
        }

        public decimal GetMenuItemOrderTagPrice()
        {
            return GetOrderTagSum(OrderTagValues.Where(x => x.CalculateWithParentPrice), TaxIncluded);
        }

        private static decimal GetOrderTagSum(IEnumerable<OrderTagValue> orderTags, bool vatIncluded)
        {
            return orderTags.Sum(orderTag => (orderTag.Price + (vatIncluded ? orderTag.TaxAmount : 0)) * orderTag.Quantity);
        }

        public void IncSelectedQuantity()
        {
            SelectedQuantity++;
            if (SelectedQuantity > Quantity) SelectedQuantity = 1;
        }

        public void DecSelectedQuantity()
        {
            SelectedQuantity--;
            if (SelectedQuantity < 1) SelectedQuantity = 1;
        }

        public void ResetSelectedQuantity()
        {
            SelectedQuantity = Quantity;
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
    }
}
