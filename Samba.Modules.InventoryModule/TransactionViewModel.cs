using System;
using System.Collections.ObjectModel;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Inventories;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    class TransactionViewModel : EntityViewModelBase<InventoryTransaction>
    {
        public TransactionViewModel(InventoryTransaction model)
            : base(model)
        {
            AddTransactionItemCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Line), OnAddTransactionItem, CanAddTransactionItem);
            DeleteTransactionItemCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Line), OnDeleteTransactionItem, CanDeleteTransactionItem);
        }

        public DateTime Date
        {
            get { return Model.Date; }
            set { Model.Date = value; }
        }

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
                     Model.TransactionItems.Select(x => new TransactionItemViewModel(x, Workspace)));
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
            return TransactionItems.Count() == 0 || CanSave(arg);
        }

        protected override bool CanSave(string arg)
        {
            return ApplicationState.IsCurrentWorkPeriodOpen && ApplicationState.CurrentWorkPeriod.StartDate < Model.Date && base.CanSave(arg);
        }

        private void OnAddTransactionItem(string obj)
        {
            var ti = new InventoryTransactionItem();
            var tiv = new TransactionItemViewModel(ti, Workspace);
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

        protected override AbstractValidator<InventoryTransaction> GetValidator()
        {
            return new TransactionValidator(ApplicationState);
        }
    }

    internal class TransactionValidator : EntityValidator<InventoryTransaction>
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
