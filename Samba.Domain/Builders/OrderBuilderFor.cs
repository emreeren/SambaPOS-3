using System.Collections.Generic;
using Samba.Domain.Models.Menus;

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

        public T Do()
        {
            _parent.Link(_orderBuilder);
            return _parent;
        }

        public OrderBuilderFor<T> ForMenuItem(MenuItem menuItem)
        {
            _orderBuilder.ForMenuItem(menuItem);
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

        public OrderBuilderFor<T> CalculatePrice(bool calculatePrice)
        {
            _orderBuilder.CalculatePrice(calculatePrice);
            return this;
        }
    }
}