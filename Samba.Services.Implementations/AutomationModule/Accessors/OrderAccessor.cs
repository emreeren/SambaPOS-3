using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.AutomationModule.Accessors
{
    public static class OrderAccessor
    {
        private static Order _model;
        public static Order Model
        {
            get { return _model ?? (_model = Order.Null); }
            set { _model = value; }
        }

        public static decimal Quantity { get { return _model.Quantity; } set { _model.Quantity = value; } }
        public static decimal Price { get { return _model.GetTotal(); } set { _model.UpdatePrice(value, ""); } }
        public static string Name { get { return Model.MenuItemName; } set { Model.MenuItemName = value; } }
    }
}