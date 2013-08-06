using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Persistance.Common;

namespace Samba.Persistance
{
    public interface IMenuDao
    {
        IEnumerable<ScreenMenu> GetScreenMenus();
        IEnumerable<string> GetMenuItemNames();
        IEnumerable<string> GetMenuItemGroupCodes();
        IEnumerable<string> GetMenuItemTags();
        IEnumerable<MenuItem> GetMenuItemsByGroupCode(string menuItemGroupCode);
        IEnumerable<MenuItem> GetMenuItems();
        IEnumerable<MenuItemData> GetMenuItemData();
        MenuItem GetMenuItemById(int id);
        IEnumerable<MenuItem> GetMenuItemsWithPortions();
    }
}
