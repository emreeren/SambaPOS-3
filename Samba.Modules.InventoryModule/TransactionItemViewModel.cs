using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.InventoryModule
{
    class TransactionItemViewModel : ObservableObject
    {
        private readonly IWorkspace _workspace;
        public TransactionItemViewModel(InventoryTransactionItem model, IWorkspace workspace)
        {
            _workspace = workspace;
            Model = model;
        }

        public InventoryTransactionItem Model { get; set; }

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
            get { return _inventoryItemNames ?? (_inventoryItemNames = Dao.Select<InventoryItem, string>(x => x.Name, x => !string.IsNullOrEmpty(x.Name))); }
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
