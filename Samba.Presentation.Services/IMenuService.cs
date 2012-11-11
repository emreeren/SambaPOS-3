using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IMenuService
    {
        MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression);
        IEnumerable<MenuItem> GetMenuItemsByGroupCode(string menuItemGroupCode);
        IEnumerable<MenuItem> GetMenuItems();
        IEnumerable<MenuItemData> GetMenuItemData();
        IEnumerable<ScreenMenu> GetScreenMenus();
        IEnumerable<string> GetScreenMenuCategories(ScreenMenuCategory category, string parentTag);
        IEnumerable<ScreenMenuItem> GetScreenMenuItems(ScreenMenuCategory category, int currentPageNo, string tag);
        IEnumerable<string> GetMenuItemNames();
        IEnumerable<string> GetMenuItemGroupCodes();
        IEnumerable<string> GetMenuItemTags();
    }
}
