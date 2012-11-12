using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Persistance;

namespace Samba.Presentation.Services
{
    public interface IMenuService
    {
        IEnumerable<MenuItem> GetMenuItemsByGroupCode(string menuItemGroupCode);
        IEnumerable<MenuItem> GetMenuItems();
        IEnumerable<MenuItemData> GetMenuItemData();
        IEnumerable<ScreenMenu> GetScreenMenus();
        IEnumerable<string> GetScreenMenuCategories(ScreenMenuCategory category, string parentTag);
        IEnumerable<ScreenMenuItem> GetScreenMenuItems(ScreenMenuCategory category, int currentPageNo, string tag);
        IEnumerable<string> GetMenuItemNames();
        IEnumerable<string> GetMenuItemGroupCodes();
        IEnumerable<string> GetMenuItemTags();
        MenuItem GetMenuItemById(int id);
    }
}
