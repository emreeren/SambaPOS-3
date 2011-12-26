using System.ComponentModel.Composition;
using Samba.Domain.Models.Inventories;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export,PartCreationPolicy(CreationPolicy.NonShared)]
    class InventoryItemListViewModel : EntityCollectionViewModelBase<InventoryItemViewModel, InventoryItem>
    {
        private readonly IInventoryService _inventoryService;

        [ImportingConstructor]
        public InventoryItemListViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        protected override string CanDeleteItem(InventoryItem model)
        {
            var item = _inventoryService.GetPeriodicConsumptionItemCountByInventoryItem(model.Id);
            if (item > 0) 
                return Resources.DeleteErrorInventoryItemUsedInEndOfDayRecord;
            return base.CanDeleteItem(model);
        }
    }
}
