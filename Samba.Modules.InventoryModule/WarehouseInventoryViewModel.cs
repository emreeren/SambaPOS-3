using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export]
    public class WarehouseInventoryViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;
        private readonly ICacheService _cacheService;

        public ICaptionCommand WarehouseButtonSelectedCommand { get; set; }

        [ImportingConstructor]
        public WarehouseInventoryViewModel(IInventoryService inventoryService, ICacheService cacheService)
        {
            _inventoryService = inventoryService;
            _cacheService = cacheService;
            WarehouseButtonSelectedCommand = new CaptionCommand<Warehouse>("", OnWarehouseSelected);
        }

        private void OnWarehouseSelected(Warehouse obj)
        {
            UpdateSelectedWarehouse(obj.Id);
        }

        public void Refresh(int warehouseId)
        {
            _warehouses = null;
            _warehouseButtons = null;
            UpdateSelectedWarehouse(warehouseId);
            RaisePropertyChanged(() => WarehouseButtons);
        }

        private void UpdateSelectedWarehouse(int warehouseId)
        {
            SelectedWarehouse = Warehouses.Single(x => x.Id == warehouseId);
            var pc = _inventoryService.GetCurrentPeriodicConsumption();
            SelectedWarehouseConsumption = pc.WarehouseConsumptions.Single(x => x.WarehouseId == SelectedWarehouse.Id);
            _periodicConsumptionItems = null;
            _costItems = null;
            RaisePropertyChanged(() => PeriodicConsumptionItems);
            RaisePropertyChanged(() => CostItems);
            RaisePropertyChanged(() => SelectedWarehouse);
        }

        private IEnumerable<Warehouse> _warehouses;
        public IEnumerable<Warehouse> Warehouses
        {
            get { return _warehouses ?? (_warehouses = _cacheService.GetWarehouses()); }
        }

        private IEnumerable<WarehouseButton> _warehouseButtons;
        public IEnumerable<WarehouseButton> WarehouseButtons
        {
            get { return _warehouseButtons ?? (_warehouseButtons = Warehouses.Select(x => new WarehouseButton(x))); }
        }

        public WarehouseConsumption SelectedWarehouseConsumption { get; set; }
        public Warehouse SelectedWarehouse { get; set; }

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

    public class WarehouseButton
    {
        public Warehouse Model { get; set; }
        public WarehouseButton(Warehouse model)
        {
            Model = model;
        }

        public string Caption { get { return Model.Name; } }
    }
}
