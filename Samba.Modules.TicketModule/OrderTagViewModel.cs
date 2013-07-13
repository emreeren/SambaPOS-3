using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class OrderTagViewModel : ObservableObject
    {
        public OrderTag Model { get; set; }
        private readonly IMenuService _menuService;

        public OrderTagViewModel(OrderTag model, IMenuService menuService)
        {
            _menuService = menuService;
            Model = model;
            if (string.IsNullOrEmpty(model.Name))
                model.Name = string.Format("[{0}]", Resources.NewProperty);
            UpdateMenuItem(model.MenuItemId);
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public decimal Price { get { return Model.Price; } set { Model.Price = value; } }
        public int MaxQuantity { get { return Model.MaxQuantity; } set { Model.MaxQuantity = value; } }

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
            get { return _menuItems ?? (_menuItems = _menuService.GetMenuItems()); }
        }

        private void UpdateMenuItem(int value)
        {
            if (value > 0)
            {
                if (MenuItem == null || MenuItem.Id != value)
                    MenuItem = MenuItems.FirstOrDefault(x => x.Id == value);
            }
        }
    }
}
