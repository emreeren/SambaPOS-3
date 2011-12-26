using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Menus;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.MenuModule
{
    [Export(typeof(IMenuService))]
    public class MenuService : AbstractService, IMenuService
    {
        public IEnumerable<ScreenMenuItem> GetMenuItems(ScreenMenuCategory category, int currentPageNo, string tag)
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

        public IEnumerable<string> GetSubCategories(ScreenMenuCategory category, string parentTag)
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

        public MenuItem GetMenuItem(int menuItemId)
        {
            return GetMenuItem(x => x.Id == menuItemId);
        }

        public MenuItem GetMenuItem(string barcode)
        {
            return GetMenuItem(x => x.Barcode == barcode);
        }

        public MenuItem GetMenuItemByName(string menuItemName)
        {
            return GetMenuItem(x => x.Name == menuItemName);
        }

        public MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression)
        {
            return Dao.SingleWithCache(expression, x => x.TaxTemplate, x => x.Portions.Select(y => y.Prices));
        }

        public IEnumerable<ScreenMenu> GetScreenMenus()
        {
            return Dao.Query<ScreenMenu>();
        }

        public IEnumerable<string> GetMenuItemNames()
        {
            return Dao.Select<MenuItem, string>(x => x.Name, null);
        }

        public override void Reset()
        {

        }
    }
}
