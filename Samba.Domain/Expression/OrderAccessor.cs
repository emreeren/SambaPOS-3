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

        public static double Quantity { get { return Convert.ToDouble(_model.Quantity); } set { _model.Quantity = Convert.ToDecimal(value); } }
        public static double Price { get { return Convert.ToDouble(_model.GetTotal()); } set { _model.UpdatePrice(Convert.ToDecimal(value), ""); } }
        public static string Name { get { return Model.MenuItemName; } set { Model.MenuItemName = value; } }
    }
}