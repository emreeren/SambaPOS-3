using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TransactionDocumentListViewModel : EntityCollectionViewModelBase<TransactionDocumentViewModel, InventoryTransactionDocument>
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public TransactionDocumentListViewModel(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        protected override bool CanAddItem(object obj)
        {
            return _applicationState.CurrentWorkPeriod != null;
        }
    }
}
