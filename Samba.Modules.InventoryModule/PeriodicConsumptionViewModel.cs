using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class PeriodicConsumptionViewModel : EntityViewModelBase<PeriodicConsumption>
    {
        private readonly IApplicationState _applicationState;
        private readonly IInventoryService _inventoryService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PeriodicConsumptionViewModel(IApplicationState applicationState,
            IInventoryService inventoryService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _inventoryService = inventoryService;
            _cacheService = cacheService;
            UpdateCalculationCommand = new CaptionCommand<string>(Resources.CalculateCost, OnUpdateCalculation);
        }

        private ObservableCollection<WarehouseConsumption> _warehouseConsumptions;
        public ObservableCollection<WarehouseConsumption> WarehouseConsumptions
        {
            get { return _warehouseConsumptions ?? (_warehouseConsumptions = new ObservableCollection<WarehouseConsumption>(Model.WarehouseConsumptions)); }
        }

        private WarehouseConsumption _selectedWarehouseConsumption;
        public WarehouseConsumption SelectedWarehouseConsumption
        {
            get { return _selectedWarehouseConsumption; }
            set
            {
                _selectedWarehouseConsumption = value;
                _periodicConsumptionItems = null;
                _costItems = null;
                RaisePropertyChanged(() => PeriodicConsumptionItems);
                RaisePropertyChanged(() => CostItems);
            }
        }

        public ICaptionCommand UpdateCalculationCommand { get; set; }

        private ObservableCollection<PeriodicConsumptionItemViewModel> _periodicConsumptionItems;
        public ObservableCollection<PeriodicConsumptionItemViewModel> PeriodicConsumptionItems
        {
            get { return _periodicConsumptionItems ?? (_periodicConsumptionItems = new ObservableCollection<PeriodicConsumptionItemViewModel>(SelectedWarehouseConsumption.PeriodicConsumptionItems.Select(x => new PeriodicConsumptionItemViewModel(x)))); }
        }

        private ObservableCollection<CostItemViewModel> _costItems;
        public ObservableCollection<CostItemViewModel> CostItems
        {
            get { return _costItems ?? (_costItems = new ObservableCollection<CostItemViewModel>(SelectedWarehouseConsumption.CostItems.Select(x => new CostItemViewModel(x, _cacheService.GetMenuItem(y => y.Id == x.MenuItemId))))); }
        }

        private PeriodicConsumptionItemViewModel _selectedPeriodicConsumptionItem;
        public PeriodicConsumptionItemViewModel SelectedPeriodicConsumptionItem
        {
            get { return _selectedPeriodicConsumptionItem; }
            set
            {
                _selectedPeriodicConsumptionItem = value;
                RaisePropertyChanged(() => SelectedPeriodicConsumptionItem);
            }
        }

        protected override bool CanSave(string arg)
        {
            return _applicationState.IsCurrentWorkPeriodOpen && _periodicConsumptionItems.Count > 0
                && Model.WorkPeriodId == _applicationState.CurrentWorkPeriod.Id && base.CanSave(arg);
        }

        private void OnUpdateCalculation(string obj)
        {
            UpdateCost();
        }

        public void UpdateCost()
        {
            _inventoryService.CalculateCost(Model, _applicationState.CurrentWorkPeriod);
            _costItems = null;
            RaisePropertyChanged(() => CostItems);
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (Model.WarehouseConsumptions.Any())
                SelectedWarehouseConsumption = Model.WarehouseConsumptions.First();
        }

        public override Type GetViewType()
        {
            return typeof(PeriodicConsumptionView);
        }

        public override string GetModelTypeString()
        {
            return Resources.EndOfDayRecord;
        }

        protected override void OnSave(string value)
        {
            _inventoryService.CalculateCost(Model, _applicationState.CurrentWorkPeriod);
            base.OnSave(value);
        }
    }
}
