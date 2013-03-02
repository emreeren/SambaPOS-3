﻿using System;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ScreenMenuItem :ValueClass, IOrderable
    {
        public ScreenMenuItem()
        {
            Quantity = 1;
            FontSize = 1;
            SubMenuTag = "";
        }

        public string Name { get; set; }
        public string UserString
        {
            get { return MenuItem != null ? MenuItem.Name + " [" + MenuItem.GroupCode + "]" : Name; }
        }

        public int ScreenMenuCategoryId { get; set; }
        public int MenuItemId { get; set; }
        public int SortOrder { get; set; }
        public bool AutoSelect { get; set; }
        public string ButtonColor { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public double FontSize { get; set; }
        public string SubMenuTag { get; set; }
        public string ItemPortion { get; set; }
        public virtual OrderTagTemplate OrderTagTemplate { get; set; }

        public MenuItem MenuItem;
    }
}
