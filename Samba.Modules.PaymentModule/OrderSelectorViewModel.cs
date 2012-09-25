using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Modules.PaymentModule
{
    public class OrderSelectorViewModel : ObservableObject
    {
        private readonly NumberPadViewModel _numberPadViewModel;

        public OrderSelectorViewModel(OrderSelector model, NumberPadViewModel numberPadViewModel)
        {
            _numberPadViewModel = numberPadViewModel;
            Model = model;
            Selectors = new ObservableCollection<SelectorViewModel>();
            SelectMergedItemCommand = new DelegateCommand<SelectorViewModel>(OnMergedItemSelected);
        }

        public DelegateCommand<SelectorViewModel> SelectMergedItemCommand { get; set; }

        protected OrderSelector Model { get; set; }
        public ObservableCollection<SelectorViewModel> Selectors { get; set; }

        public decimal SelectedTotal { get { return Model.SelectedTotal; } }
        public decimal RemainingTotal { get { return Model.RemainingTotal; } }

        public void UpdateTicket(Ticket ticket)
        {
            Model.UpdateTicket(ticket);
            Selectors.Clear();
            Selectors.AddRange(Model.Selectors.Select(x => new SelectorViewModel(x)));
            Refresh();
        }

        public void PersistSelectedItems()
        {
            Model.PersistSelectedItems();
            Refresh();
        }

        public void PersistTicket()
        {
            Model.PersistTicket();
            Refresh();
        }

        public void ClearSelection()
        {
            Model.ClearSelection();
            Refresh();
        }

        public void UpdateExchangeRate(decimal exchangeRate)
        {
            Model.UpdateExchangeRate(exchangeRate);
            Refresh();
        }

        private void Refresh()
        {
            Selectors.ToList().ForEach(x => x.Refresh());
        }

        public void UpdateAutoRoundValue(decimal autoRoundDiscount)
        {
            Model.UpdateAutoRoundValue(autoRoundDiscount);
        }

        private void OnMergedItemSelected(SelectorViewModel obj)
        {
            obj.Select();
            var paymentAmount = SelectedTotal;
            var remaining = decimal.Round(Model.GetRemainingAmount() / Model.ExchangeRate, 2);
            if (Math.Abs(remaining - paymentAmount) <= 0.01m)
                paymentAmount = remaining;
            _numberPadViewModel.PaymentDueAmount = paymentAmount.ToString("#,#0.00");
            _numberPadViewModel.TenderedAmount = "";
            _numberPadViewModel.ResetAmount = true;
        }
    }
}