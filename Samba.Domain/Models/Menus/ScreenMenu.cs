using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ScreenMenu : EntityClass
    {
        public ScreenMenu()
            : this("Menu")
        {

        }

        public ScreenMenu(string name)
        {
            Name = name;
            _categories = new List<ScreenMenuCategory>();
            CategoryColumnWidthRate = 25;
            CategoryColumnCount = 1;
        }

        public int CategoryColumnCount { get; set; }
        public int CategoryColumnWidthRate { get; set; }

        private IList<ScreenMenuCategory> _categories;
        public virtual IList<ScreenMenuCategory> Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        public ScreenMenuCategory AddCategory(string categoryName)
        {
            var result = new ScreenMenuCategory(categoryName);
            Categories.Add(result);
            return result;
        }

        public string UserString
        {
            get { return Name; }
        }

        public void AddItemsToCategory(string category, List<MenuItem> menuItems)
        {
            ScreenMenuCategory menuCategory = Categories.Single(x => x.Name == category);
            Debug.Assert(menuCategory != null);
            foreach (var menuItem in menuItems)
            {
                menuCategory.AddMenuItem(menuItem);
            }
        }
    }
}
