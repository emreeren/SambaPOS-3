using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.TicketModule
{
    public class OrderTagViewModel : ObservableObject
    {
        public OrderTag Model { get; set; }
        private readonly IEnumerable<Order> _selectedOrders;

        public OrderTagViewModel(OrderTag model)
            : this(null, model)
        {
        }

        public OrderTagViewModel(IEnumerable<Order> selectedOrder, OrderTag model)
        {
            _selectedOrders = selectedOrder;
            Model = model;
            if (string.IsNullOrEmpty(model.Name))
                model.Name = string.Format("[{0}]", Resources.NewProperty);
            UpdateMenuItem(model.MenuItemId);
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public decimal Price { get { return Model.Price; } set { Model.Price = value; } }
        public string Color
        {
            get
            {
                if (_selectedOrders != null && _selectedOrders.All(x => x.IsTaggedWith(Model)))
                    return "Red";
                return "Transparent";
            }
        }

        public int MenuItemId
        {
            get { return Model.MenuItemId; }
            set
            {
                Model.MenuItemId = value;
                UpdateMenuItem(value);
            }
        }

        private MenuItem _menuItem;
        public MenuItem MenuItem
        {
            get
            {
                return _menuItem;
            }
            set
            {
                _menuItem = value;
                MenuItemId = value.Id;
            }
        }

        private IEnumerable<MenuItem> _menuItems;
        public IEnumerable<MenuItem> MenuItems
        {
            get { return _menuItems ?? (_menuItems = Dao.Query<MenuItem>()); }
        }

        private void UpdateMenuItem(int value)
        {
            if (value > 0)
            {
                if (MenuItem == null || MenuItem.Id != value)
                    MenuItem = Dao.Single<MenuItem>(x => x.Id == value, x => x.Portions);
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => Color);
        }
    }
}
