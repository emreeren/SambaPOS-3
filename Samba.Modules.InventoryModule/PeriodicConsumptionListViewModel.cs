using System;
using System.Linq;
using Samba.Domain.Models.Inventories;
using Samba.Localization.Properties;
using Samba.Modules.InventoryModule.ServiceImplementations;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    class PeriodicConsumptionListViewModel : EntityCollectionViewModelBase<PeriodicConsumptionViewModel, PeriodicConsumption>
    {
        protected override PeriodicConsumptionViewModel CreateNewViewModel(PeriodicConsumption model)
        {
            return new PeriodicConsumptionViewModel(model);
        }

        protected override PeriodicConsumption CreateNewModel()
        {
            return new PeriodicConsumption();
        }

        protected override void OnAddItem(object obj)
        {
            var pc = InventoryService.GetCurrentPeriodicConsumption(Workspace);
            VisibleViewModelBase wm = Items.SingleOrDefault(x => x.Name == pc.Name) ?? InternalCreateNewViewModel(pc);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override bool CanAddItem(object obj)
        {
            return WorkPeriodService.CurrentWorkPeriod != null;
        }

        protected override string CanDeleteItem(PeriodicConsumption model)
        {
            if (model.WorkPeriodId != WorkPeriodService.CurrentWorkPeriod.Id
                || !WorkPeriodService.IsCurrentWorkPeriodOpen)
                return Resources.CantDeletePastEndOfDayRecords;
            return base.CanDeleteItem(model);
        }
    }
}
