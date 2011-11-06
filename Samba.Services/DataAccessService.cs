using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tables;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class DataAccessService
    {
        public IEnumerable<Table> GetCurrentTables(int tableScreenId, int currentPageNo)
        {
            AppServices.MainDataContext.UpdateTables(tableScreenId, currentPageNo);

            var selectedTableScreen = AppServices.MainDataContext.SelectedTableScreen;

            if (selectedTableScreen != null)
            {
                if (selectedTableScreen.PageCount > 1)
                {
                    return selectedTableScreen.Tables
                         .OrderBy(x => x.Order)
                         .Skip(selectedTableScreen.ItemCountPerPage * currentPageNo)
                         .Take(selectedTableScreen.ItemCountPerPage);
                }
                return selectedTableScreen.Tables;
            }
            return new List<Table>();
        }

        public IEnumerable<ScreenMenuItem> GetMenuItems(ScreenMenuCategory category, int currentPageNo, string tag)
        {
            var items = category.ScreenMenuItems
                .Where(x => x.Tag == tag || (string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(x.Tag)));

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
            return category.ScreenMenuItems.Where(x => !string.IsNullOrEmpty(x.Tag))
                .Select(x => x.Tag)
                .Distinct()
                .Where(x => string.IsNullOrEmpty(parentTag) || (x.StartsWith(parentTag) && x != parentTag))
                .Select(x => Regex.Replace(x, "^" + parentTag + ",", ""))
                .Where(x => !x.Contains(","))
                .Select(x => !string.IsNullOrEmpty(parentTag) ? parentTag + "," + x : x);
        }

        public ScreenMenu GetScreenMenu(int screenMenuId)
        {
            return Dao.SingleWithCache<ScreenMenu>(x => x.Id == screenMenuId, x => x.Categories,
                                          x => x.Categories.Select(z => z.ScreenMenuItems));
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
            return Dao.SingleWithCache(expression, x => x.TaxTemplate, x => x.PropertyGroups.Select(z => z.Properties), x => x.Portions.Select(y => y.Prices));
        }

        public IEnumerable<string> GetInventoryItemNames()
        {
            return Dao.Select<InventoryItem, string>(x => x.Name, x => !string.IsNullOrEmpty(x.Name));
        }

        public Table GetTable(string tableName)
        {
            return Dao.Single<Table>(x => x.Name == tableName);
        }
    }
}
