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

        public ICaptionCommand AddUnmappedItemsCommand { get; set; }

        [ImportingConstructor]
        public PeriodicConsumptionViewModel(IApplicationState applicationState,
            IInventoryService inventoryService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _inventoryService = inventoryService;
            _cacheService = cacheService;
            UpdateCalculationCommand = new CaptionCommand<string>(Resources.CalculateCost, OnUpdateCalculation);
            AddUnmappedItemsCommand = new CaptionCommand<string>(Resources.AppendUnmappedItems, OnAddUnmappedItems, CanSave);
        }

        private ObservableCollection<WarehouseConsumptionViewModel> _warehouseConsumptions;
        public ObservableCollection<WarehouseConsumptionViewModel> WarehouseConsumptions
        {
            get
            {
                return _warehouseConsumptions ?? (_warehouseConsumptions =
                    new ObservableCollection<WarehouseConsumptionViewModel>(Model.WarehouseConsumptions.Select(x => new WarehouseConsumptionViewModel(x, _cacheService, _inventoryService))));
            }
        }

        private WarehouseConsumptionViewModel _selectedWarehouseConsumption;
        public WarehouseConsumptionViewModel SelectedWarehouseConsumption
        {
            get { return _selectedWarehouseConsumption; }
            set
            {
                _selectedWarehouseConsumption = value;
                _selectedWarehouseConsumption.Refresh();
                RaisePropertyChanged(() => SelectedWarehouseConsumption);
            }
        }

        public ICaptionCommand UpdateCalculationCommand { get; set; }

        public string NameStr { get { return String.Format(Resources.Period_f, Name); } }

        private void OnAddUnmappedItems(string obj)
        {
            SelectedWarehouseConsumption.AddMissingItems();
        }

        protected override bool CanSave(string arg)
        {
            return !_applicationState.IsCurrentWorkPeriodOpen && SelectedWarehouseConsumption != null
                && SelectedWarehouseConsumption.PeriodicConsumptionItems.Count > 0
                && Model.WorkPeriodId == _applicationState.CurrentWorkPeriod.Id && base.CanSave(arg);
        }

        private void OnUpdateCalculation(string obj)
        {
            UpdateCost();
        }

        public void UpdateCost()
        {
            _inventoryService.CalculateCost(Model, _applicationState.CurrentWorkPeriod);
            SelectedWarehouseConsumption.Refresh();
        }

        public override void OnShown()
        {
            base.OnShown();
            if (SelectedWarehouseConsumption == null && WarehouseConsumptions.Any())
                SelectedWarehouseConsumption = WarehouseConsumptions.First();
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
            _inventoryService.FilterUnneededItems(Model);
            _inventoryService.CalculateCost(Model, _applicationState.CurrentWorkPeriod);
            base.OnSave(value);
        }
    }
}
