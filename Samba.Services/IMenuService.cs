using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Menus;

namespace Samba.Services
{
    public interface IMenuService
    {
        IEnumerable<ScreenMenuItem> GetMenuItems(ScreenMenuCategory category, int currentPageNo, string tag);
        IEnumerable<string> GetSubCategories(ScreenMenuCategory category, string parentTag);
        ScreenMenu GetScreenMenu(int screenMenuId);
        MenuItem GetMenuItem(int menuItemId);
        MenuItem GetMenuItem(string barcode);
        MenuItem GetMenuItemByName(string menuItemName);
        MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression);
    }
}
