using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Builders
{
    public class OrderBuilderFor<T> : ILinkableToMenuItemBuilder<OrderBuilderFor<T>> where T : ILinkableToOrderBuilder<T>
    {
        private readonly T _parent;
        private readonly OrderBuilder _orderBuilder;

        private OrderBuilderFor(T parent)
        {
            _parent = parent;
            _orderBuilder = OrderBuilder.Create();
        }

        public static OrderBuilderFor<T> Create(T parent)
        {
            return new OrderBuilderFor<T>(parent);
        }

        public OrderBuilderFor<T> ForMenuItem(MenuItem menuItem)
        {
            _orderBuilder.ForMenuItem(menuItem);
            return this;
        }

        public OrderBuilderFor<T> WithQuantity(decimal quantity)
        {
            _orderBuilder.WithQuantity(quantity);
            return this;
        }

        public OrderBuilderFor<T> WithTaxTemplates(IList<TaxTemplate> taxTemplates)
        {
            _orderBuilder.WithTaxTemplates(taxTemplates);
            return this;
        }
        
        public OrderBuilderFor<T> WithTaxTemplate(TaxTemplate taxTemplate)
        {
            _orderBuilder.AddTaxTemplate(taxTemplate);
            return this;
        }

        public OrderBuilderFor<T> CalculatePrice(bool calculatePrice)
        {
            _orderBuilder.CalculatePrice(calculatePrice);
            return this;
        }

        public OrderBuilderFor<T> WithPrice(decimal price)
        {
            _orderBuilder.WithPrice(price);
            return this;
        }

        public OrderBuilderFor<T> ToggleOrderTag(OrderTagGroup orderTagGroup, OrderTag orderTag)
        {
            _orderBuilder.ToggleOrderTag(orderTagGroup, orderTag);
            return this;
        }

        public void Link(MenuItem menuItem)
        {
            _orderBuilder.ForMenuItem(menuItem);
        }

        public MenuItemBuilderFor<OrderBuilderFor<T>> CreateMenuItem(string menuItemName)
        {
            return MenuItemBuilderFor<OrderBuilderFor<T>>.Create(menuItemName, this);
        }

        public T Do(int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                _parent.Link(_orderBuilder);
            }
            return _parent;
        }
    }
}