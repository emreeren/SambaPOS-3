using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class TransactionDocumentViewModel : EntityViewModelBase<InventoryTransactionDocument>
    {
        public event EventHandler RowInserted;
        public event EventHandler RowDeleted;

        private readonly IApplicationState _applicationState;
        private readonly IInventoryService _inventoryService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public TransactionDocumentViewModel(IApplicationState applicationState, IInventoryService inventoryService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _inventoryService = inventoryService;
            _cacheService = cacheService;
            AddTransactionItemCommand = new CaptionCommand<string>(Resources.Add, OnAddTransactionItem, CanAddTransactionItem);
            DeleteTransactionItemCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteTransactionItem, CanDeleteTransactionItem);
        }

        public DateTime Date
        {
            get { return Model.Date; }
            set { Model.Date = value; }
        }

        public string DateTimeStr { get { return string.Format(Resources.Date + ": {0}", Model.Date); } }

        public ICaptionCommand AddTransactionItemCommand { get; set; }
        public ICaptionCommand DeleteTransactionItemCommand { get; set; }

        private ObservableCollection<TransactionViewModel> _transactionItems;
        public ObservableCollection<TransactionViewModel> TransactionItems
        {
            get { return _transactionItems ?? (_transactionItems = CreateTransactionItemModels()); }
        }

        private ObservableCollection<TransactionViewModel> CreateTransactionItemModels()
        {
            return new ObservableCollection<TransactionViewModel>(Model.TransactionItems.Select(x => new TransactionViewModel(x, Workspace, _inventoryService, _cacheService)));
        }

        private TransactionViewModel _selectedTransactionItem;
        public TransactionViewModel SelectedTransactionItem
        {
            get { return _selectedTransactionItem; }
            set
            {
                _selectedTransactionItem = value;
                RaisePropertyChanged(() => SelectedTransactionItem);
            }
        }

        private bool CanDeleteTransactionItem(string arg)
        {
            return SelectedTransactionItem != null && (CanSave(arg) || SelectedTransactionItem.InventoryItem == null);
        }

        private void OnDeleteTransactionItem(string obj)
        {
            if (SelectedTransactionItem.Model.Id > 0)
                Workspace.Delete(SelectedTransactionItem.Model);
            Model.TransactionItems.Remove(SelectedTransactionItem.Model);
            TransactionItems.Remove(SelectedTransactionItem);
            OnRowDeleted();
        }

        private bool CanAddTransactionItem(string arg)
        {
            return !TransactionItems.Any() || CanSave(arg);
        }

        protected override bool CanSave(string arg)
        {
            return _applicationState.IsCurrentWorkPeriodOpen && _applicationState.CurrentWorkPeriod.StartDate < Model.Date && base.CanSave(arg);
        }

        private void OnAddTransactionItem(string obj)
        {
            var inventoryTransactionType = InventoryTransactionType.Default;
            var lt = TransactionItems.LastOrDefault();
            if (lt != null) inventoryTransactionType = lt.InventoryTransactionType;

            var ti = Model.Add(inventoryTransactionType, null, 0, 0, "", 1);
            var tiv = new TransactionViewModel(ti, Workspace, _inventoryService, _cacheService);
            TransactionItems.Add(tiv);
            SelectedTransactionItem = tiv;
            RaisePropertyChanged(() => TransactionItems);
            OnRowInserted();
        }

        protected override void OnSave(string value)
        {
            var modified = false;
            foreach (var transactionItemViewModel in _transactionItems)
            {
                if (transactionItemViewModel.Model.InventoryItem == null || transactionItemViewModel.Quantity == 0)
                {
                    modified = true;
                    Model.TransactionItems.Remove(transactionItemViewModel.Model);
                    if (transactionItemViewModel.Model.Id > 0)
                        Workspace.Delete(transactionItemViewModel.Model);
                }
            }
            if (modified) _transactionItems = null;
            base.OnSave(value);
        }

        public void OnRowDeleted()
        {
            EventHandler handler = RowDeleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void OnRowInserted()
        {
            EventHandler handler = RowInserted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public override Type GetViewType()
        {
            return typeof(TransactionDocumentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TransactionDocument;
        }

        protected override AbstractValidator<InventoryTransactionDocument> GetValidator()
        {
            return new TransactionValidator(_applicationState);
        }

        public void ExecuteTransactionItemCommand()
        {
            if (AddTransactionItemCommand.CanExecute(""))
                AddTransactionItemCommand.Execute("");
        }
    }

    internal class TransactionValidator : EntityValidator<InventoryTransactionDocument>
    {
        public TransactionValidator(IApplicationState applicationState)
        {
            var startDate = applicationState.IsCurrentWorkPeriodOpen
                                ? applicationState.CurrentWorkPeriod.StartDate
                                : DateTime.Now;
            RuleFor(x => x.Date).GreaterThan(startDate);
            RuleFor(x => x.TransactionItems).Must(x => x.Count > 0).WithMessage(Resources.TransactionsEmptyError)
            .Must(x => x.Count(y => y.Quantity == 0) == 0).WithMessage(Resources.TranactionsZeroQuantityError)
            .Must(x => x.Count(y => y.Multiplier == 0) == 0).WithMessage(Resources.TransactionMultiplierError)
            .Must(x => x.Count(y => string.IsNullOrEmpty(y.Unit)) == 0).WithMessage(Resources.TransactionUnitError);
        }
    }
}
