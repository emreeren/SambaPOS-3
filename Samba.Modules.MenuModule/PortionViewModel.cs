using Samba.Domain.Models.Menus;
using Samba.Presentation.Common;

namespace Samba.Modules.MenuModule
{
    public class PortionViewModel : ObservableObject
    {
        public PortionViewModel(MenuItemPortion model)
        {
            Model = model;
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public decimal Price { get { return Model.Price.Amount; } set { Model.Price.Amount = value; } }
        public int Multiplier { get { return Model.Multiplier; } set { Model.Multiplier = value; } }
        public string CurrencyCode { get { return Model.Price.CurrencyCode; } set { Model.Price.CurrencyCode = value; } }
        public MenuItemPortion Model { get; private set; }
    }
}
