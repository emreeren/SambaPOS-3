using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Diagnostics;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Settings;

namespace Samba.Domain.Models.Tickets
{
    public class TicketItem
    {
        public TicketItem()
        {
            _properties = new List<TicketItemProperty>();
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
        public string CurrencyCode { get; set; }
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

        private IList<TicketItemProperty> _properties;
        public virtual IList<TicketItemProperty> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        decimal _selectedQuantity;
        public decimal SelectedQuantity { get { return _selectedQuantity; } }

        public void UpdateMenuItem(int userId, MenuItem menuItem, string portionName, string priceTag, int quantity, string defaultProperties)
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

            if (!string.IsNullOrEmpty(defaultProperties))
            {
                foreach (var menuItemPropertyGroup in menuItem.PropertyGroups)
                {
                    var properties = defaultProperties.Split(',');
                    foreach (var defaultProperty in properties)
                    {
                        var property = defaultProperty.Trim();
                        var defaultValue = menuItemPropertyGroup.Properties.FirstOrDefault(x => x.Name == property);
                        if (defaultValue != null)
                            ToggleProperty(menuItemPropertyGroup, defaultValue);
                    }
                }
            }
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

            CurrencyCode = LocalSettings.CurrencySymbol;
            foreach (var ticketItemProperty in Properties)
            {
                ticketItemProperty.PortionName = portion.Name;
            }
        }

        public void ToggleProperty(MenuItemPropertyGroup group, MenuItemProperty property)
        {
            if (group.MultipleSelection && property.Price == 0)
            {
                var groupItems = Properties.Where(x => x.PropertyGroupId == group.Id).ToList();
                foreach (var tip in groupItems) Properties.Remove(tip);
                Quantity = 1;
                return;
            }

            var ti = FindProperty(property.Name);
            if (ti == null)
            {
                ti = new TicketItemProperty
                        {
                            Name = property.Name,
                            Price = property.Price,
                            PropertyGroupId = group.Id,
                            MenuItemId = property.MenuItemId,
                            CalculateWithParentPrice = group.CalculateWithParentPrice,
                            PortionName = PortionName,
                            Quantity = group.MultipleSelection ? 0 : 1
                        };

                if (TaxIncluded && TaxRate > 0)
                {
                    ti.Price = ti.Price / ((100 + TaxRate) / 100);
                    ti.Price = decimal.Round(ti.Price, 2);
                    ti.TaxAmount = property.Price - ti.Price;
                }
                else if (TaxRate > 0) ti.TaxAmount = (property.Price * TaxRate) / 100;
                else ti.TaxAmount = 0;
            }
            if (group.SingleSelection)
            {
                var tip = Properties.FirstOrDefault(x => x.PropertyGroupId == group.Id);
                if (tip != null)
                {
                    Properties.Insert(Properties.IndexOf(tip), ti);
                    Properties.Remove(tip);
                }
            }
            else if (group.MultipleSelection)
            {
                ti.Quantity++;
            }
            else if (!group.MultipleSelection && Properties.Contains(ti))
            {
                Properties.Remove(ti);
                return;
            }

            if (!Properties.Contains(ti)) Properties.Add(ti);
        }

        public TicketItemProperty GetCustomProperty()
        {
            return Properties.FirstOrDefault(x => x.PropertyGroupId == 0);
        }

        public TicketItemProperty GetOrCreateCustomProperty()
        {
            var tip = GetCustomProperty();
            if (tip == null)
            {
                tip = new TicketItemProperty
                          {
                              Name = "",
                              Price = 0,
                              PropertyGroupId = 0,
                              MenuItemId = 0,
                              Quantity = 0
                          };
                Properties.Add(tip);
            }
            return tip;
        }

        public void UpdateCustomProperty(string text, decimal price, decimal quantity)
        {
            var tip = GetOrCreateCustomProperty();
            if (string.IsNullOrEmpty(text))
            {
                Properties.Remove(tip);
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

        private TicketItemProperty FindProperty(string propertyName)
        {
            return Properties.FirstOrDefault(x => x.Name == propertyName);
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
            var result = Price + GetTotalPropertyPrice();
            if (TaxIncluded) result += TaxAmount;
            return result;
        }

        public decimal GetTotalPropertyPrice()
        {
            return GetPropertySum(Properties, TaxIncluded);
        }

        public decimal GetPropertyPrice()
        {
            return GetPropertySum(Properties.Where(x => !x.CalculateWithParentPrice), TaxIncluded);
        }

        public decimal GetMenuItemPropertyPrice()
        {
            return GetPropertySum(Properties.Where(x => x.CalculateWithParentPrice), TaxIncluded);
        }

        private static decimal GetPropertySum(IEnumerable<TicketItemProperty> properties, bool vatIncluded)
        {
            return properties.Sum(property => (property.Price + (vatIncluded ? property.TaxAmount : 0)) * property.Quantity);
        }

        public void IncSelectedQuantity()
        {
            _selectedQuantity++;
            if (_selectedQuantity > Quantity) _selectedQuantity = 1;
        }

        public void DecSelectedQuantity()
        {
            _selectedQuantity--;
            if (_selectedQuantity < 1) _selectedQuantity = 1;
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
    }
}
