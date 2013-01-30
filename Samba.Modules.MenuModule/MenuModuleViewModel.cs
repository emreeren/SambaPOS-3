using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [Export]
    public class MenuModuleViewModel
    {
        private readonly ICacheService _cacheService;
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public MenuModuleViewModel(ICacheService cacheService, IMenuService menuService)
        {
            _cacheService = cacheService;
            _menuService = menuService;
        }

        private IEnumerable<MenuItem> _menuItems;
        public IEnumerable<MenuItem> MenuItems
        {
            get { return _menuItems ?? (_menuItems = GetMenuItems()); }
        }

        private IEnumerable<MenuItem> GetMenuItems()
        {
            return _menuService.GetMenuItems();
        }
    }
}
