using System.Collections.Generic;
using Samba.Domain.Models.Inventory;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Services;

namespace Samba.Modules.InventoryModule
{
    class RecipeItemViewModel : ObservableObject
    {
        private readonly IWorkspace _workspace;
        private readonly IInventoryService _inventoryService;

        public RecipeItemViewModel(RecipeItem model, IWorkspace workspace, IInventoryService inventoryService)
        {
            Model = model;
            _workspace = workspace;
            _inventoryService = inventoryService;
        }

        public RecipeItem Model { get; set; }
        public InventoryItem InventoryItem { get { return Model.InventoryItem; } set { Model.InventoryItem = value; } }

        private IEnumerable<string> _inventoryItemNames;
        public IEnumerable<string> InventoryItemNames
        {
            get { return _inventoryItemNames ?? (_inventoryItemNames = _inventoryService.GetInventoryItemNames()); }
        }

        public string Name
        {
            get
            {
                return Model.InventoryItem != null ? Model.InventoryItem.Name : string.Format("- {0} -", Localization.Properties.Resources.Select);
            }
            set
            {
                UpdateInventoryItem(value);
                RaisePropertyChanged(() => Name);
                RaisePropertyChanged(() => UnitName);
            }
        }

        public string UnitName
        {
            get
            {
                return Model.InventoryItem != null ? Model.InventoryItem.BaseUnit : "";
            }
        }

        public decimal Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                RaisePropertyChanged(() => Quantity);
            }
        }

        private void UpdateInventoryItem(string value)
        {
            var i = _workspace.Single<InventoryItem>(x => x.Name.ToLower() == value.ToLower());
            InventoryItem = i;
        }
    }
}
