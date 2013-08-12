using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ScreenMenuItem : ValueClass, IOrderable
    {
        public ScreenMenuItem()
        {
            Quantity = 1;
            FontSize = 1;
            SubMenuTag = "";
        }

        public string UserString
        {
            get { return MenuItem != null ? MenuItem.Name + " [" + MenuItem.GroupCode + "]" : Name; }
        }

        public string Caption { get { return Name != null ? Name.Replace("\\r", " ") : ""; } }

        public string Name { get; set; }
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
        public MenuItem MenuItem;
        public string OrderTags { get; set; }
        public string OrderStates { get; set; }
        public string AutomationCommand { get; set; }
        public string AutomationCommandValue { get; set; }
    }
}
