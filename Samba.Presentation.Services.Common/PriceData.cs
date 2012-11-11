using Samba.Domain.Models.Menus;

namespace Samba.Presentation.Services.Common
{
    public class PriceData
    {
        public MenuItemPortion Portion { get; set; }
        public string Name { get; set; }

        public PriceData(MenuItemPortion portion, string name)
        {
            Portion = portion;
            Name = name;
        }
    }
}