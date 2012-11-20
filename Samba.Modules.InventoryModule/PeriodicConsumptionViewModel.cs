using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventories;
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
        private readonly IPresentationCacheService _cacheService;

        [ImportingConstructor]
        public PeriodicConsumptionViewModel(IApplicationState applicationState,
            IInventoryService inventoryService, IPresentationCacheService cacheService)
        {
            _applicationState = applicationState;
            _inventoryService = inventoryService;
            _cacheService = cacheService;
            UpdateCalculationCommand = new CaptionCommand<string>(Resources.CalculateCost, OnUpdateCalculation);
        }

        public ICaptionCommand UpdateCalculationCommand { get; set; }

        private ObservableCollection<PeriodicConsumptionItemViewModel> _periodicConsumptionItems;
        public ObservableCollection<PeriodicConsumptionItemViewModel> PeriodicConsumptionItems
        {
            get { return _periodicConsumptionItems ?? (_periodicConsumptionItems = new ObservableCollection<PeriodicConsumptionItemViewModel>(Model.PeriodicConsumptionItems.Select(x => new PeriodicConsumptionItemViewModel(x)))); }
        }

        private ObservableCollection<CostItemViewModel> _costItems;
        public ObservableCollection<CostItemViewModel> CostItems
        {
            get { return _costItems ?? (_costItems = new ObservableCollection<CostItemViewModel>(Model.CostItems.Select(x => new CostItemViewModel(x, _cacheService.GetMenuItem(y => y.Id == x.Portion.MenuItemId))))); }
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
