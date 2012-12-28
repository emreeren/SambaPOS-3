using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export]
    public class ResourceInventoryViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public ResourceInventoryViewModel(IInventoryService inventoryService, ICacheService cacheService)
        {
            _inventoryService = inventoryService;
            _cacheService = cacheService;
        }

        public void Refresh(Resource resource)
        {
            SelectedResource = resource;
            var pc = _inventoryService.GetCurrentPeriodicConsumption();
            SelectedWarehouseConsumption = pc.WarehouseConsumptions.Single(x => x.WarehouseId == SelectedResource.Id);
            _periodicConsumptionItems = null;
            _costItems = null;
            RaisePropertyChanged(() => PeriodicConsumptionItems);
            RaisePropertyChanged(() => CostItems);
        }

        public WarehouseConsumption SelectedWarehouseConsumption { get; set; }
        protected Resource SelectedResource { get; set; }

        private ObservableCollection<PeriodicConsumptionItemViewModel> _periodicConsumptionItems;
        public ObservableCollection<PeriodicConsumptionItemViewModel> PeriodicConsumptionItems
        {
            get { return _periodicConsumptionItems ?? (_periodicConsumptionItems = CreatePeriodicConsumptionItems()); }
        }

        private ObservableCollection<CostItemViewModel> _costItems;
        public ObservableCollection<CostItemViewModel> CostItems
        {
            get { return _costItems ?? (_costItems = CreateCostItems()); }
        }

        private ObservableCollection<CostItemViewModel> CreateCostItems()
        {
            if (SelectedWarehouseConsumption == null) return null;
            return
                new ObservableCollection<CostItemViewModel>(
                    SelectedWarehouseConsumption.CostItems.Select(
                        x => new CostItemViewModel(x, _cacheService.GetMenuItem(y => y.Id == x.MenuItemId))));
        }

        private ObservableCollection<PeriodicConsumptionItemViewModel> CreatePeriodicConsumptionItems()
        {
            if (SelectedWarehouseConsumption == null) return null;
            return new ObservableCollection<PeriodicConsumptionItemViewModel>(
                    SelectedWarehouseConsumption.PeriodicConsumptionItems.Select(
                        x => new PeriodicConsumptionItemViewModel(x)));
        }
    }
}
