using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Builders
{
    public class OrderBuilder : ILinkableToMenuItemBuilder<OrderBuilder>
    {
        private MenuItem _menuItem;
        private MenuItemPortion _portion;
        private string _userName;
        private string _priceTag;
        private decimal _quantity;
        private Department _department;
        private AccountTransactionType _accountTransactionType;
        private ProductTimer _productTimer;
        private bool _calculatePrice;
        private decimal? _price;

        private readonly IList<TaxTemplate> _taxTemplates;
        private readonly IList<OrderTagData> _orderTags;

        public static OrderBuilder Create()
        {
            return new OrderBuilder();
        }

        public static OrderBuilder Create(AccountTransactionType accountTransactionType, Department department)
        {
            var result = Create();
            result.WithDepartment(department);
            result.WithAccountTransactionType(accountTransactionType);
            return result;
        }

        public OrderBuilder()
        {
            _priceTag = "";
            _quantity = 1;
            _taxTemplates = new List<TaxTemplate>();
            _orderTags = new List<OrderTagData>();
            _calculatePrice = true;
        }

        public Order Build()
        {
            if (_department == null) throw new ArgumentNullException("Department");
            if (_accountTransactionType == null) throw new ArgumentNullException("AccountTransactionType");
            if (_menuItem == null) throw new ArgumentNullException("MenuItem");

            var result = new Order();
            result.UpdateMenuItem(_userName, _menuItem, _taxTemplates, _portion, _priceTag, _quantity);
            result.DepartmentId = _department.Id;
            result.WarehouseId = _department.WarehouseId;
            result.AccountTransactionTypeId = _accountTransactionType.Id;
            result.UpdateProductTimer(_productTimer);
            result.CalculatePrice = _calculatePrice;
            result.Price = _price.GetValueOrDefault(result.Price);

            foreach (var orderTagData in _orderTags)
            {
                result.ToggleOrderTag(orderTagData.OrderTagGroup, orderTagData.OrderTag, 1, "");
            }

            return result;
        }

        public OrderBuilder ForMenuItem(MenuItem menuItem)
        {
            _menuItem = menuItem;
            if (menuItem.Portions.Any() && _portion == null)
            {
                WithPortion(menuItem.Portions[0]);
            }
            return this;
        }

        public OrderBuilder WithPortion(MenuItemPortion portion)
        {
            _portion = portion;
            return this;
        }

        public OrderBuilder WithUserName(string userName)
        {
            _userName = userName;
            return this;
        }

        public OrderBuilder WithPriceTag(string priceTag)
        {
            _priceTag = priceTag;
            return this;
        }

        public OrderBuilder WithQuantity(decimal quantity)
        {
            _quantity = quantity;
            return this;
        }

        public OrderBuilder WithDepartment(Department department)
        {
            _department = department;
            return this;
        }

        public OrderBuilder WithTaxTemplates(IEnumerable<TaxTemplate> taxTemplate)
        {
            if (taxTemplate != null)
            {
                foreach (var template in taxTemplate)
                {
                    AddTaxTemplate(template);
                }
            }
            return this;
        }

        public OrderBuilder AddTaxTemplate(TaxTemplate taxTemplate)
        {
            _taxTemplates.Add(taxTemplate);
            return this;
        }

        public OrderBuilder WithAccountTransactionType(AccountTransactionType accountTransactionType)
        {
            _accountTransactionType = accountTransactionType;
            return this;
        }

        public OrderBuilder WithProductTimer(ProductTimer productTimer)
        {
            _productTimer = productTimer;
            return this;
        }

        public void Link(MenuItem menuItem)
        {
            ForMenuItem(menuItem);
        }

        public MenuItemBuilderFor<OrderBuilder> CreateMenuItem(string menuItemName)
        {
            return MenuItemBuilderFor<OrderBuilder>.Create(menuItemName, this);
        }

        public IEnumerable<TaxTemplate> GetTaxTemplates()
        {
            return _taxTemplates;
        }

        public AccountTransactionType GetTransactionType()
        {
            return _accountTransactionType;
        }

        public string GetUserName()
        {
            return _userName;
        }

        public OrderBuilder CalculatePrice(bool calculatePrice)
        {
            _calculatePrice = calculatePrice;
            return this;
        }

        public OrderBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public OrderBuilder ToggleOrderTag(OrderTagGroup orderTagGroup, OrderTag orderTag)
        {
            _orderTags.Add(new OrderTagData {OrderTagGroup = orderTagGroup, OrderTag = orderTag});
            return this;
        }

        
    }

    internal class OrderTagData
    {
        public OrderTagGroup OrderTagGroup { get; set; }
        public OrderTag OrderTag { get; set; }
    }
}