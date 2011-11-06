using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Presentation.Common;

namespace Samba.Modules.InventoryModule
{
    class PeriodicConsumptionItemViewModel : ObservableObject
    {
        public PeriodicConsumptionItemViewModel(PeriodicConsumptionItem model)
        {
            Model = model;
        }
        public PeriodicConsumptionItem Model { get; set; }

        public string ItemName { get { return Model.InventoryItem.Name; } }
        public string UnitName { get { return Model.InventoryItem.TransactionUnitMultiplier > 0 ? Model.InventoryItem.TransactionUnit : Model.InventoryItem.BaseUnit; } }
        public decimal InStock { get { return Model.InStock; } }
        public decimal Purchase { get { return Model.Purchase; } }
        public decimal Cost { get { return Model.Cost; } }
        public decimal Consumption { get { return Model.Consumption; } }
        public decimal InventoryPrediction { get { return Model.GetInventoryPrediction(); } }
        public decimal? PhysicalInventory
        {
            get { return Model.PhysicalInventory; }
            set
            {
                Model.PhysicalInventory = value;
                RaisePropertyChanged(()=>PhysicalInventory);
            }
        }
    }
}
