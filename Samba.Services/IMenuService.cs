using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Menus;

namespace Samba.Services
{
    public class MenuItemData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupCode { get; set; }
    }

    public interface IMenuService
    {
        ScreenMenu GetScreenMenu(int screenMenuId);
        IEnumerable<ScreenMenu> GetScreenMenus();
        IEnumerable<string> GetScreenMenuCategories(ScreenMenuCategory category, string parentTag);
        IEnumerable<ScreenMenuItem> GetMenuItems(ScreenMenuCategory category, int currentPageNo, string tag);
        MenuItem GetMenuItemById(int menuItemId);
        MenuItem GetMenuItemByBarcode(string barcode);
        MenuItem GetMenuItemByName(string menuItemName);
        MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression);
        IEnumerable<string> GetMenuItemNames();
        IEnumerable<string> GetMenuItemGroupCodes();
        IEnumerable<string> GetMenuItemTags();
        IEnumerable<MenuItem> GetMenuItemsByGroupCode(string menuItemGroupCode);
        IEnumerable<MenuItem> GetMenuItems();
        IEnumerable<MenuItemData> GetMenuItemData();
    }
}
