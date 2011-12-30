using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Menus;

namespace Samba.Services
{
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
    }
}
