using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Persistance;
using Samba.Persistance.Common;

namespace Samba.Services.Implementations.MenuModule
{
    [Export(typeof(IMenuService))]
    public class MenuService : IMenuService
    {
        private readonly IMenuDao _menuDao;

        [ImportingConstructor]
        public MenuService(IMenuDao menuDao)
        {
            _menuDao = menuDao;
        }

        public IEnumerable<ScreenMenu> GetScreenMenus()
        {
            return _menuDao.GetScreenMenus();
        }

        public IEnumerable<string> GetMenuItemNames()
        {
            return _menuDao.GetMenuItemNames();
        }

        public IEnumerable<string> GetMenuItemGroupCodes()
        {
            return _menuDao.GetMenuItemGroupCodes();
        }

        public IEnumerable<string> GetMenuItemTags()
        {
            return _menuDao.GetMenuItemTags();
        }

        public MenuItem GetMenuItemById(int id)
        {
            return _menuDao.GetMenuItemById(id);
        }

        public IEnumerable<MenuItem> GetMenuItemsByGroupCode(string menuItemGroupCode)
        {
            return _menuDao.GetMenuItemsByGroupCode(menuItemGroupCode);
        }

        public IEnumerable<MenuItem> GetMenuItems()
        {
            return _menuDao.GetMenuItems();
        }        
        
        public IEnumerable<MenuItem> GetMenuItemsWithPortions()
        {
            return _menuDao.GetMenuItemsWithPortions();
        }

        public IEnumerable<MenuItemData> GetMenuItemData()
        {
            return _menuDao.GetMenuItemData();
        }
    }
}
