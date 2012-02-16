using System.ComponentModel.Composition;
using Samba.Domain.Models.Inventories;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TransactionListViewModel : EntityCollectionViewModelBase<TransactionViewModel, InventoryTransaction>
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public TransactionListViewModel(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        protected override bool CanAddItem(object obj)
        {
            return _applicationState.CurrentWorkPeriod != null;
        }
    }
}
