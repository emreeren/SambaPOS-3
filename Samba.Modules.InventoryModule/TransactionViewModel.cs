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
    class TransactionViewModel : EntityViewModelBase<InventoryTransactionDocument>
    {
        private readonly IApplicationState _applicationState;
        private readonly IInventoryService _inventoryService;

        [ImportingConstructor]
        public TransactionViewModel(IApplicationState applicationState, IInventoryService inventoryService)
        {
            _applicationState = applicationState;
            _inventoryService = inventoryService;
            AddTransactionItemCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Line), OnAddTransactionItem, CanAddTransactionItem);
            DeleteTransactionItemCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Line), OnDeleteTransactionItem, CanDeleteTransactionItem);
        }

        public DateTime Date
        {
            get { return Model.Date; }
            set { Model.Date = value; }
        }

        public string DateStr { get { return string.Format(Resources.Date + ": {0:d}", Model.Date); } }
        public string TimeStr { get { return string.Format(Resources.Time + ": {0:t}", Model.Date); } }

        public ICaptionCommand AddTransactionItemCommand { get; set; }
        public ICaptionCommand DeleteTransactionItemCommand { get; set; }

        private ObservableCollection<TransactionItemViewModel> _transactionItems;
        public ObservableCollection<TransactionItemViewModel> TransactionItems
        {
            get { return _transactionItems ?? (_transactionItems = GetTransactionItems()); }
        }

        private ObservableCollection<TransactionItemViewModel> GetTransactionItems()
        {
            if (Model.TransactionItems.Count == 0)
                AddTransactionItemCommand.Execute("");
            return new ObservableCollection<TransactionItemViewModel>(
                     Model.TransactionItems.Select(x => new TransactionItemViewModel(x, Workspace, _inventoryService)));
        }

        private TransactionItemViewModel _selectedTransactionItem;
        public TransactionItemViewModel SelectedTransactionItem
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
            return SelectedTransactionItem != null && CanSave(arg);
        }

        private void OnDeleteTransactionItem(string obj)
        {
            if (SelectedTransactionItem.Model.Id > 0)
                Workspace.Delete(SelectedTransactionItem.Model);
            Model.TransactionItems.Remove(SelectedTransactionItem.Model);
            TransactionItems.Remove(SelectedTransactionItem);
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
            var ti = new InventoryTransaction();
            var tiv = new TransactionItemViewModel(ti, Workspace, _inventoryService);
            Model.TransactionItems.Add(ti);
            TransactionItems.Add(tiv);
            SelectedTransactionItem = tiv;
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

        public override Type GetViewType()
        {
            return typeof(TransactionView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TransactionDocument;
        }

        protected override AbstractValidator<InventoryTransactionDocument> GetValidator()
        {
            return new TransactionValidator(_applicationState);
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
