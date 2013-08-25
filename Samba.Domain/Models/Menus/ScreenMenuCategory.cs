using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ScreenMenuCategory : ValueClass, IOrderable
    {
        public ScreenMenuCategory()
        {
            _screenMenuItems = new List<ScreenMenuItem>();
            MainButtonHeight = 65;
            MenuItemButtonHeight = 65;
            SubButtonHeight = 65;
            ColumnCount = 0;
            WrapText = false;
            MenuItemButtonColor = "Green";
            MainButtonColor = "Orange";
            NumeratorType = 2;
            PageCount = 1;
            MainFontSize = 30;
            MenuItemFontSize = 30;
            SubButtonRows = 1;
            SubButtonColorDef = "";
        }

        public ScreenMenuCategory(string name)
            : this()
        {
            Name = name;
        }

        public string Name { get; set; }

        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }

        public int ScreenMenuId { get; set; }

        public bool MostUsedItemsCategory { get; set; }

        private IList<ScreenMenuItem> _screenMenuItems;
        public virtual IList<ScreenMenuItem> ScreenMenuItems
        {
            get { return _screenMenuItems; }
            set { _screenMenuItems = value; }
        }

        public int MenuItemCount { get { return ScreenMenuItems.Count; } }
        public int ColumnCount { get; set; }

        public int MenuItemButtonHeight { get; set; }
        public string MenuItemButtonColor { get; set; }
        public double MenuItemFontSize { get; set; }

        public bool WrapText { get; set; }
        public int PageCount { get; set; }

        public int MainButtonHeight { get; set; }
        public string MainButtonColor { get; set; }
        public double MainFontSize { get; set; }

        public int SubButtonHeight { get; set; }
        public int SubButtonRows { get; set; }
        public string SubButtonColorDef { get; set; }

        public int NumeratorType { get; set; }
        public string NumeratorValues { get; set; }
        public string AlphaButtonValues { get; set; }

        public string ImagePath { get; set; }

        public bool IsQuickNumeratorVisible { get { return NumeratorType == 1; } }
        public bool IsNumeratorVisible { get { return NumeratorType == 2; } }

        public int MaxItems { get; set; }

        public void AddMenuItem(MenuItem menuItem)
        {
            var smi = new ScreenMenuItem { MenuItemId = menuItem.Id, Name = menuItem.Name };
            ScreenMenuItems.Add(smi);
        }

        public int ItemCountPerPage
        {
            get
            {
                var itemCount = ScreenMenuItems.Count / PageCount;
                if (ScreenMenuItems.Count % PageCount > 0) itemCount++;
                return itemCount;
            }
        }

        public IEnumerable<ScreenMenuItem> GetScreenMenuItems(int currentPageNo, string tag)
        {
            var items = ScreenMenuItems.Where(x => x.SubMenuTag == tag || (string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(x.SubMenuTag)));

            if (PageCount > 1)
            {
                items = items
                    .Skip(ItemCountPerPage * currentPageNo)
                    .Take(ItemCountPerPage);
            }

            return items.OrderBy(x => x.SortOrder);
        }

        public IEnumerable<string> GetScreenMenuCategories(string parentTag)
        {
            return ScreenMenuItems.Where(x => !string.IsNullOrEmpty(x.SubMenuTag))
                .Select(x => x.SubMenuTag)
                .Distinct()
                .Where(x => string.IsNullOrEmpty(parentTag) || (x.StartsWith(parentTag) && x != parentTag))
                .Select(x => Regex.Replace(x, "^" + parentTag + ",", ""))
                .Where(x => !x.Contains(","))
                .Select(x => !string.IsNullOrEmpty(parentTag) ? parentTag + "," + x : x);
        }
    }
}
