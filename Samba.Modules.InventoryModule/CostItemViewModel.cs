using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common;

namespace Samba.Modules.InventoryModule
{
    public class CostItemViewModel : ObservableObject
    {
        public CostItem Model { get; set; }

        public CostItemViewModel(CostItem model,MenuItem menuItem)
        {
            Model = model;
            _menuItem = menuItem;
        }

        private readonly MenuItem _menuItem;
        public MenuItem MenuItem { get { return _menuItem; } }
        public string MenuItemName { get { return MenuItem.Name; } }
        public string PortionName { get { return Model.PortionName; } }
        public decimal Quantity { get { return Model.Quantity; } }
        public decimal CostPrediction { get { return Model.CostPrediction; } }
        public decimal Cost { get { return Model.Cost; } }
    }
}
