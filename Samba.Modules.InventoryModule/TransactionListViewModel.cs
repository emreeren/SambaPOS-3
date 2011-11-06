using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

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
            return AppServices.MainDataContext.CurrentWorkPeriod != null;
        }
    }
}
