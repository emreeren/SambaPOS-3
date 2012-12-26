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
    class TransactionListViewModel : EntityCollectionViewModelBase<TransactionViewModel, InventoryTransaction>
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public TransactionListViewModel(IApplicationState applicationState)
        {
            _applicationState = applicationState;
            RemoveCommand(AddItemCommand);
            foreach (var transactionType in Workspace.All<InventoryTransactionType>(x => x.AccountTransactionType).OrderByDescending(x => x.SortOrder))
            {
                InsertCommand(new CustomCommand(string.Format(Resources.Add_f, transactionType.Name), OnExecute, transactionType, CanExecute), 0);
            }
        }

        private bool CanExecute(object arg)
        {
            return true;
        }

        private void OnExecute(object obj)
        {
            var transactionType = obj as InventoryTransactionType;
            var result = InventoryTransaction.Create(transactionType);
            PublishViewModel(result);
        }

        protected override bool CanAddItem(object obj)
        {
            return _applicationState.CurrentWorkPeriod != null;
        }
    }
}
