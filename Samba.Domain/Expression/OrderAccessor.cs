using System;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Expression
{
    public static class OrderAccessor
    {
        private static Order _model;
        public static Order Model
        {
            get { return _model ?? (_model = Order.Null); }
            set { _model = value; }
        }

        public static decimal Quantity { get { return Model.Quantity; } set { Model.Quantity = value; } }
        public static decimal Price { get { return Model.GetPrice(); } set { Model.UpdatePrice(value, ""); } }
        public static string Name { get { return Model.MenuItemName; } set { Model.MenuItemName = value; } }
    }
}