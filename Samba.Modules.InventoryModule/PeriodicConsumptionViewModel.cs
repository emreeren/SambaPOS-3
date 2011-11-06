using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    class PeriodicConsumptionViewModel : EntityViewModelBase<PeriodicConsumption>
    {
        public PeriodicConsumptionViewModel(PeriodicConsumption model)
            : base(model)
        {
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
            get { return _costItems ?? (_costItems = new ObservableCollection<CostItemViewModel>(Model.CostItems.Select(x => new CostItemViewModel(x)))); }
        }

        private PeriodicConsumptionItemViewModel _selectedPeriodicConsumptionItem;
        public PeriodicConsumptionItemViewModel SelectedPeriodicConsumptionItem
        {
            get { return _selectedPeriodicConsumptionItem; }
            set
            {
                _selectedPeriodicConsumptionItem = value;
                RaisePropertyChanged(()=>SelectedPeriodicConsumptionItem);
            }
        }

        protected override bool CanSave(string arg)
        {
            return !AppServices.MainDataContext.IsCurrentWorkPeriodOpen && _periodicConsumptionItems.Count > 0
                && Model.WorkPeriodId == AppServices.MainDataContext.CurrentWorkPeriod.Id && base.CanSave(arg);
        }
        
        private void OnUpdateCalculation(string obj)
        {
            UpdateCost();
        }

        public void UpdateCost()
        {
            InventoryService.CalculateCost(Model, AppServices.MainDataContext.CurrentWorkPeriod);
            _costItems = null;
            RaisePropertyChanged(()=>CostItems);
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
            InventoryService.CalculateCost(Model, AppServices.MainDataContext.CurrentWorkPeriod);
            base.OnSave(value);
        }
    }
}
