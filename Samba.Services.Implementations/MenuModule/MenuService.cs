using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.MenuModule
{
    [Export(typeof(IMenuService))]
    public class MenuService : AbstractService, IMenuService
    {
        [ImportingConstructor]
        public MenuService()
        {
            ValidatorRegistry.RegisterDeleteValidator(new MenuItemDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new ScreenMenuDeleteValidator());
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

        public ScreenMenu GetScreenMenu(int screenMenuId)
        {
            return Dao.SingleWithCache<ScreenMenu>(x => x.Id == screenMenuId, x => x.Categories,
            x => x.Categories.Select(z => z.ScreenMenuItems.Select(
                w => w.OrderTagTemplate.OrderTagTemplateValues.Select(
                    x1 => x1.OrderTag)))
            ,
            x => x.Categories.Select(z => z.ScreenMenuItems.Select(
                w => w.OrderTagTemplate.OrderTagTemplateValues.Select(
                    x1 => x1.OrderTagGroup))));
        }

        public MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression)
        {
            return Dao.Single(expression, x => x.TaxTemplate, x => x.Portions.Select(y => y.Prices));
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

        public override void Reset()
        {

        }
    }

    public class ScreenMenuDeleteValidator : SpecificationValidator<ScreenMenu>
    {
        public override string GetErrorMessage(ScreenMenu model)
        {
            if (Dao.Exists<Department>(x => x.ScreenMenuId == model.Id))
                return Resources.DeleteErrorMenuViewUsedInDepartment;
            return "";
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
