using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Menus;
using Samba.Persistance;
using Samba.Persistance.DaoClasses;

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

        public IEnumerable<ScreenMenuItem> GetScreenMenuItems(ScreenMenuCategory category, int currentPageNo, string tag)
        {
            var items = category.ScreenMenuItems
                .Where(x => x.SubMenuTag == tag || (string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(x.SubMenuTag)));

            if (category.PageCount > 1)
            {
                items = items
                    .Skip(category.ItemCountPerPage * currentPageNo)
                    .Take(category.ItemCountPerPage);
            }

            return items.OrderBy(x => x.Order);
        }

        public IEnumerable<string> GetScreenMenuCategories(ScreenMenuCategory category, string parentTag)
        {
            return category.ScreenMenuItems.Where(x => !string.IsNullOrEmpty(x.SubMenuTag))
                .Select(x => x.SubMenuTag)
                .Distinct()
                .Where(x => string.IsNullOrEmpty(parentTag) || (x.StartsWith(parentTag) && x != parentTag))
                .Select(x => Regex.Replace(x, "^" + parentTag + ",", ""))
                .Where(x => !x.Contains(","))
                .Select(x => !string.IsNullOrEmpty(parentTag) ? parentTag + "," + x : x);
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

        public IEnumerable<MenuItemData> GetMenuItemData()
        {
            return _menuDao.GetMenuItemData();
        }
    }
}
