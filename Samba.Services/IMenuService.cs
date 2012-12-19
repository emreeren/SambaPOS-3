using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Persistance;

namespace Samba.Services
{
    public interface IMenuService
    {
        IEnumerable<MenuItem> GetMenuItemsByGroupCode(string menuItemGroupCode);
        IEnumerable<MenuItem> GetMenuItems();
        IEnumerable<MenuItemData> GetMenuItemData();
        IEnumerable<ScreenMenu> GetScreenMenus();
        IEnumerable<string> GetMenuItemNames();
        IEnumerable<string> GetMenuItemGroupCodes();
        IEnumerable<string> GetMenuItemTags();
        MenuItem GetMenuItemById(int id);
    }
}
