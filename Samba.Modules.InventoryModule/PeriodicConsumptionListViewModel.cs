using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class PeriodicConsumptionListViewModel : EntityCollectionViewModelBase<PeriodicConsumptionViewModel, PeriodicConsumption>
    {
        private readonly IApplicationState _applicationState;
        private readonly IInventoryService _inventoryService;

        [ImportingConstructor]
        public PeriodicConsumptionListViewModel(IApplicationState applicationState, IInventoryService inventoryService)
        {
            _applicationState = applicationState;
            _inventoryService = inventoryService;
        }

        protected override void OnAddItem(object obj)
        {
            var pc = _inventoryService.GetCurrentPeriodicConsumption();
            VisibleViewModelBase wm = Items.SingleOrDefault(x => x.Name == pc.Name) ?? InternalCreateNewViewModel(pc);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override bool CanAddItem(object obj)
        {
            return _applicationState.CurrentWorkPeriod != null;
        }

        protected override string CanDeleteItem(PeriodicConsumption model)
        {
            if (model.WorkPeriodId != _applicationState.CurrentWorkPeriod.Id
                || !_applicationState.IsCurrentWorkPeriodOpen)
                return Resources.CantDeletePastEndOfDayRecords;
            return base.CanDeleteItem(model);
        }

        //protected override System.Collections.Generic.IEnumerable<PeriodicConsumption> SelectItems()
        //{
        //    var filter = (Filter ?? "").ToLower();
        //    return !string.IsNullOrEmpty(filter)
        //        ? Workspace.All<PeriodicConsumption>(x => x.Name.ToLower().Contains(filter)).OrderByDescending(x => x.EndDate).Take(Limit)
        //        : Workspace.All<PeriodicConsumption>().OrderByDescending(x => x.EndDate).Take(Limit);
        //}
    }
}
