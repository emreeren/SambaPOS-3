using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventory;
using Samba.Presentation.Common;

namespace Samba.Modules.InventoryModule
{
    public class PeriodicConsumptionItemViewModel : ObservableObject
    {
        public PeriodicConsumptionItemViewModel(PeriodicConsumptionItem model)
        {
            Model = model;
        }
        public PeriodicConsumptionItem Model { get; set; }

        public string ItemName { get { return Model.InventoryItemName; } }
        public string UnitName { get { return Model.UnitName; } }
        public decimal InStock { get { return Model.InStock; } }
        public decimal Purchase { get { return Model.Added; } }
        public decimal Cost { get { return Model.Cost; } }
        public decimal Consumption { get { return Model.Consumption + Model.Removed; } }
        public decimal InventoryPrediction { get { return Model.GetInventoryPrediction(); } }
        public decimal? PhysicalInventory
        {
            get { return Model.PhysicalInventory; }
            set
            {
                Model.PhysicalInventory = value;
                RaisePropertyChanged(() => PhysicalInventory);
            }
        }
    }
}
