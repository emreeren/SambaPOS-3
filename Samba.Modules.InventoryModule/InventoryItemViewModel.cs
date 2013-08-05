using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class InventoryItemViewModel : EntityViewModelBase<InventoryItem>
    {
        private readonly IInventoryService _inventoryService;

        [ImportingConstructor]
        public InventoryItemViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public override Type GetViewType()
        {
            return typeof(InventoryItemView);
        }

        public override string GetModelTypeString()
        {
            return Resources.InventoryItem;
        }

        private IEnumerable<string> _groupCodes;
        public IEnumerable<string> GroupCodes { get { return _groupCodes ?? (_groupCodes = _inventoryService.GetGroupCodes()); } }

        private IEnumerable<string> _warehouseNames;
        public IEnumerable<string> WarehouseNames
        {
            get { return _warehouseNames ?? (_warehouseNames = _inventoryService.GetWarehouseNames()); }
        }

        public string GroupCode
        {
            get { return Model.GroupCode ?? ""; }
            set { Model.GroupCode = value; }
        }

        public string Warehouse
        {
            get { return Model.Warehouse ?? ""; }
            set { Model.Warehouse = value; }
        }

        public string BaseUnit
        {
            get { return Model.BaseUnit; }
            set
            {
                Model.BaseUnit = value; RaisePropertyChanged(() => BaseUnit);
                RaisePropertyChanged(() => PredictionUnit);
            }
        }

        public string TransactionUnit
        {
            get { return Model.TransactionUnit; }
            set
            {
                Model.TransactionUnit = value;
                RaisePropertyChanged(() => TransactionUnit);
                RaisePropertyChanged(() => PredictionUnit);
            }
        }

        public int TransactionUnitMultiplier
        {
            get { return Model.TransactionUnitMultiplier; }
            set
            {
                Model.TransactionUnitMultiplier = value;
                RaisePropertyChanged(() => TransactionUnitMultiplier);
                RaisePropertyChanged(() => PredictionUnit);
            }
        }

        public string PredictionUnit
        {
            get
            {
                return TransactionUnitMultiplier > 0 ? TransactionUnit : BaseUnit;
            }
        }

        public string GroupValue { get { return Model.GroupCode; } }

    }
}
