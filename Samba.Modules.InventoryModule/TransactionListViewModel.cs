using Samba.Domain.Models.Inventories;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    class TransactionListViewModel : EntityCollectionViewModelBase<TransactionViewModel, InventoryTransaction>
    {
        protected override TransactionViewModel CreateNewViewModel(InventoryTransaction model)
        {
            return new TransactionViewModel(model);
        }

        protected override InventoryTransaction CreateNewModel()
        {
            return new InventoryTransaction();
        }

        protected override bool CanAddItem(object obj)
        {
            return ApplicationState.CurrentWorkPeriod != null;
        }
    }
}
