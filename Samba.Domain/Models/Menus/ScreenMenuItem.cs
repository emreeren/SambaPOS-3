using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ScreenMenuItem : IOrderable
    {
        public ScreenMenuItem()
        {
            Quantity = 1;
            Tag = "";
        }

        public int Id { get; set; }

        public string Name { get; set; }
        public string UserString
        {
            get { return MenuItem != null ? MenuItem.Name + " [" + MenuItem.GroupCode + "]" : Name; }
        }

        public int ScreenMenuCategoryId { get; set; }
        public int MenuItemId { get; set; }
        public int Order { get; set; }
        public bool AutoSelect { get; set; }
        public string ButtonColor { get; set; }
        public int Quantity { get; set; }
        public bool Gift { get; set; }
        public string ImagePath { get; set; }
        public string Tag { get; set; }
        public string ItemPortion { get; set; }
        public int UsageCount { get; set; }

        public MenuItem MenuItem;
    }
}
