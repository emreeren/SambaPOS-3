using Samba.Domain.Models.Menus;

namespace Samba.Modules.MenuModule
{
    public class MenuItemPriceViewModel
    {
        public MenuItemPrice Model { get; set; }

        public MenuItemPriceViewModel(MenuItemPrice model)
        {
            Model = model;
        }

        public int Id { get { return Model.Id; } set { Model.Id = value; } }
        public int MenuItemPortionId { get { return Model.MenuItemPortionId; } set { Model.MenuItemPortionId = value; } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }
        public decimal Price { get { return Model.Price; } set { Model.Price = value; } }
    }
}
