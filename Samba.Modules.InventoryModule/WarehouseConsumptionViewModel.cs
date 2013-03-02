using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    class WarehouseConsumptionViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;

        public WarehouseConsumptionViewModel(WarehouseConsumption model, ICacheService cacheService)
        {
            _cacheService = cacheService;
            Model = model;
        }

        private string _name;
        public string Name { get { return _name ?? (_name = _cacheService.GetWarehouses().Single(x => x.Id == Model.WarehouseId).Name); } }

        protected WarehouseConsumption Model { get; set; }

        private ObservableCollection<PeriodicConsumptionItemViewModel> _periodicConsumptionItems;
        public ObservableCollection<PeriodicConsumptionItemViewModel> PeriodicConsumptionItems
        {
            get { return _periodicConsumptionItems ?? (_periodicConsumptionItems = new ObservableCollection<PeriodicConsumptionItemViewModel>(Model.PeriodicConsumptionItems.Select(x => new PeriodicConsumptionItemViewModel(x)))); }
        }

        private ObservableCollection<CostItemViewModel> _costItems;
        public ObservableCollection<CostItemViewModel> CostItems
        {
            get { return _costItems ?? (_costItems = new ObservableCollection<CostItemViewModel>(Model.CostItems.Select(x => new CostItemViewModel(x, _cacheService.GetMenuItem(y => y.Id == x.MenuItemId))))); }
        }

        public void Refresh()
        {
            _periodicConsumptionItems = null;
            _costItems = null;
            RaisePropertyChanged(() => PeriodicConsumptionItems);
            RaisePropertyChanged(() => CostItems);
        }
    }
}
