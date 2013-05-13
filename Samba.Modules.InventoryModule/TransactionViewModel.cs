using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    public class TransactionViewModel : ObservableObject
    {
        private readonly IWorkspace _workspace;
        private readonly IInventoryService _inventoryService;
        private readonly ICacheService _cacheService;

        public TransactionViewModel(InventoryTransaction model, IWorkspace workspace, IInventoryService inventoryService, ICacheService cacheService)
        {
            _workspace = workspace;
            _inventoryService = inventoryService;
            _cacheService = cacheService;
            Model = model;
            UpdateWarehouses();
        }

        public InventoryTransaction Model { get; set; }

        private static IEnumerable<Warehouse> _emptyList;
        public static IEnumerable<Warehouse> EmptyList
        {
            get { return _emptyList ?? (_emptyList = new List<Warehouse>()); }
        }

        private IEnumerable<InventoryTransactionType> _inventoryTransactionTypes;
        public IEnumerable<InventoryTransactionType> InventoryTransactionTypes
        {
            get { return _inventoryTransactionTypes ?? (_inventoryTransactionTypes = _cacheService.GetInventoryTransactionTypes()); }
        }

        public IEnumerable<Warehouse> SourceWarehouses { get; set; }
        public IEnumerable<Warehouse> TargetWarehouses { get; set; }

        public InventoryTransactionType InventoryTransactionType
        {
            get { return InventoryTransactionTypes.SingleOrDefault(x => x.Id == Model.InventoryTransactionTypeId); }
            set
            {
                Model.InventoryTransactionTypeId = value != null ? value.Id : 0;
                UpdateWarehouses();
            }
        }

        private void UpdateWarehouses()
        {
            var itt = InventoryTransactionType;
            if (itt != null)
            {
                SourceWarehouses = _cacheService.GetWarehouses().Where(x => x.WarehouseTypeId == itt.SourceWarehouseTypeId);
                TargetWarehouses = _cacheService.GetWarehouses().Where(x => x.WarehouseTypeId == itt.TargetWarehouseTypeId);
                SourceWarehouse = SourceWarehouses.SingleOrDefault(x => x.Id == itt.DefaultSourceWarehouseId);
                TargetWarehouse = TargetWarehouses.SingleOrDefault(x => x.Id == itt.DefaultTargetWarehouseId);
            }
            else
            {
                SourceWarehouses = EmptyList;
                TargetWarehouses = EmptyList;
            }
        }

        public Warehouse SourceWarehouse
        {
            get { return SourceWarehouses.SingleOrDefault(x => x.Id == Model.SourceWarehouseId); }
            set
            {
                Model.SourceWarehouseId = value != null ? value.Id : 0;
                RaisePropertyChanged(() => SourceWarehouse);
            }
        }

        public Warehouse TargetWarehouse
        {
            get { return TargetWarehouses.SingleOrDefault(x => x.Id == Model.TargetWarehouseId); }
            set
            {
                Model.TargetWarehouseId = value != null ? value.Id : 0;
                RaisePropertyChanged(() => TargetWarehouse);
            }
        }

        public InventoryItem InventoryItem
        {
            get { return Model.InventoryItem; }
            set
            {
                if (value != null)
                {
                    Model.InventoryItem = value;
                    UnitName = value.TransactionUnitMultiplier > 0 ? value.TransactionUnit : value.BaseUnit;
                }
            }
        }

        public string Name
        {
            get
            {
                return Model.InventoryItem != null
                           ? Model.InventoryItem.Name
                           : string.Format("- {0} -", Localization.Properties.Resources.Select);
            }
            set
            {
                UpdateInventoryItem(value);
                RaisePropertyChanged(() => Name);
                RaisePropertyChanged(() => UnitName);
                RaisePropertyChanged(() => UnitNames);
            }
        }

        public string UnitName
        {
            get { return Model.Unit; }
            set
            {
                Model.Unit = value;
                Model.Multiplier = value == InventoryItem.TransactionUnit ? InventoryItem.TransactionUnitMultiplier : 1;
                RaisePropertyChanged(() => UnitName);
            }
        }

        private IEnumerable<string> _inventoryItemNames;
        public IEnumerable<string> InventoryItemNames
        {
            get { return _inventoryItemNames ?? (_inventoryItemNames = _inventoryService.GetInventoryItemNames()); }
        }

        public IEnumerable<string> UnitNames
        {
            get
            {
                if (Model.InventoryItem != null)
                {
                    var result = new List<string> { Model.InventoryItem.BaseUnit };
                    if (Model.InventoryItem.TransactionUnitMultiplier > 0)
                        result.Add(Model.InventoryItem.TransactionUnit);
                    return result;
                }
                return new List<string>();
            }
        }

        public decimal Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                RaisePropertyChanged(() => Quantity);
                RaisePropertyChanged(() => TotalPrice);
            }
        }

        public decimal Price
        {
            get { return Model.Price; }
            set
            {
                Model.Price = value;
                RaisePropertyChanged(() => Price);
                RaisePropertyChanged(() => TotalPrice);
            }
        }

        public decimal TotalPrice
        {
            get { return Model.Price * Model.Quantity; }
            set
            {
                Model.Price = (value / Model.Quantity);
                RaisePropertyChanged(() => Price);
                RaisePropertyChanged(() => TotalPrice);
            }
        }

        private void UpdateInventoryItem(string value)
        {
            InventoryItem = _workspace.Single<InventoryItem>(x => x.Name.ToLower() == value.ToLower());
        }
    }
}
