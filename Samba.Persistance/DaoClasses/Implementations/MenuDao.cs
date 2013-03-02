using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.DaoClasses.Implementations
{
    [Export(typeof(IMenuDao))]
    class MenuDao : IMenuDao
    {
        [ImportingConstructor]
        public MenuDao()
        {
            ValidatorRegistry.RegisterDeleteValidator(new MenuItemDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator<ScreenMenu>(x => Dao.Exists<TicketType>(y => y.ScreenMenuId == x.Id), Resources.Menu, Resources.TicketType);
        }

        public IEnumerable<ScreenMenu> GetScreenMenus()
        {
            return Dao.Query<ScreenMenu>();
        }

        public IEnumerable<string> GetMenuItemNames()
        {
            return Dao.Select<MenuItem, string>(x => x.Name, null);
        }

        public IEnumerable<string> GetMenuItemGroupCodes()
        {
            return Dao.Distinct<MenuItem>(x => x.GroupCode);
        }

        public IEnumerable<string> GetMenuItemTags()
        {
            return Dao.Distinct<MenuItem>(x => x.Tag);
        }

        public IEnumerable<MenuItem> GetMenuItemsByGroupCode(string menuItemGroupCode)
        {
            return Dao.Query<MenuItem>(x => x.GroupCode == menuItemGroupCode);
        }

        public IEnumerable<MenuItem> GetMenuItems()
        {
            return Dao.Query<MenuItem>();
        }

        public IEnumerable<MenuItemData> GetMenuItemData()
        {
            return Dao.Select<MenuItem, MenuItemData>(
                    x => new MenuItemData { Id = x.Id, GroupCode = x.GroupCode, Name = x.Name }, x => x.Id > 0);
        }

        public MenuItem GetMenuItemById(int id)
        {
            return Dao.Single<MenuItem>(x => x.Id == id, x => x.Portions.Select(y => y.Prices));
        }
    }

    public class MenuItemDeleteValidator : SpecificationValidator<MenuItem>
    {
        public override string GetErrorMessage(MenuItem model)
        {
            if (Dao.Exists<ScreenMenuItem>(x => x.MenuItemId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.MenuItem, Resources.Menu);
            if (Dao.Exists<Recipe>(x => x.Portion.MenuItemId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.MenuItem, Resources.Recipe);
            if (Dao.Exists<OrderTag>(x => x.MenuItemId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.MenuItem, Resources.OrderTag);
            return "";
        }
    }
}
