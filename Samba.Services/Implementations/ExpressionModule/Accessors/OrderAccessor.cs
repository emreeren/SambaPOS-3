using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.ExpressionModule.Accessors
{
    public static class OrderAccessor
    {
        private static Order _model;
        public static Order Model
        {
            get { return _model ?? (_model = Order.Null); }
            set { _model = value; }
        }

        public static decimal Quantity { get { return Model.Quantity; } set { Model.Quantity = value; Model.ResetSelectedQuantity(); } }
        public static decimal Price { get { return Model.GetPrice(); } set { Model.UpdatePrice(value, ""); } }
        public static string Name { get { return Model.MenuItemName; } set { Model.MenuItemName = value; } }
        public static string PriceTag { get { return Model.PriceTag; } }
        public static string PortionName { get { return Model.PortionName; } }

        public static bool IsInState(string stateName)
        {
            return Model.IsInState(stateName);
        }
    }
}