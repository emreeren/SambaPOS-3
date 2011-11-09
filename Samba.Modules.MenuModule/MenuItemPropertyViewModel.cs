using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.MenuModule
{
    public class MenuItemPropertyViewModel : ObservableObject
    {
        public MenuItemProperty Model { get; set; }

        public MenuItemPropertyViewModel(MenuItemProperty model)
        {
            Model = model;
            if (string.IsNullOrEmpty(model.Name))
                model.Name = string.Format("[{0}]", Resources.NewProperty);
            UpdateMenuItem(model.MenuItemId);
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public decimal Price { get { return Model.Price; } set { Model.Price= value; } }

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
                    MenuItem = Dao.Single<MenuItem>(x => x.Id == value, x => x.PropertyGroups, x => x.Portions);
            }
        }

    }
}
