using System.Collections.Generic;
using Samba.Domain.Models.Inventories;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    public class MaterialViewModel : ObservableObject
    {
        public InventoryItem Model { get; set; }
        private readonly IInventoryService _inventoryService;
        private readonly IWorkspace _workspace;

        public MaterialViewModel(InventoryItem model, IWorkspace workspace, IInventoryService inventoryService)
        {
            Model = model;
            _workspace = workspace;
            _inventoryService = inventoryService;
        }

        private IEnumerable<string> _inventoryItemNames;
        public IEnumerable<string> InventoryItemNames
        {
            get { return _inventoryItemNames ?? (_inventoryItemNames = _inventoryService.GetInventoryItemNames()); }
        }

        public string Name
        {
            get
            {
                return Model != null ? Model.Name : string.Format("- {0} -", Resources.Select);
            }
            set
            {
                UpdateInventoryItem(value);
                RaisePropertyChanged(() => Name);
            }
        }

        private void UpdateInventoryItem(string name)
        {
            Model = _workspace.Single<InventoryItem>(x => x.Name.ToLower() == name.ToLower());
        }
    }
}
