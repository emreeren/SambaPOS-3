using System;
using System.ComponentModel.Composition;
using System.Globalization;
using Microsoft.Practices.Prism.Commands;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class NumberPadViewModel : ObservableObject
    {
        private readonly PaymentEditor _paymentEditor;
        private readonly TenderedValueViewModel _tenderedValueViewModel;
        private readonly OrderSelectorViewModel _orderSelectorViewModel;
        private readonly AccountBalances _accountBalances;
        private readonly ForeignCurrencyButtonsViewModel _foreignCurrencyButtonsViewModel;
        private readonly TicketTotalsViewModel _paymentTotals;

        [ImportingConstructor]
        public NumberPadViewModel(PaymentEditor paymentEditor, TenderedValueViewModel tenderedValueViewModel,
            OrderSelectorViewModel orderSelectorViewModel, AccountBalances accountBalances,
            ForeignCurrencyButtonsViewModel foreignCurrencyButtonsViewModel,TicketTotalsViewModel paymentTotals)
        {
            _paymentEditor = paymentEditor;
            _tenderedValueViewModel = tenderedValueViewModel;
            _orderSelectorViewModel = orderSelectorViewModel;
            _accountBalances = accountBalances;
            _foreignCurrencyButtonsViewModel = foreignCurrencyButtonsViewModel;
            _paymentTotals = paymentTotals;
            TenderAllCommand = new CaptionCommand<string>(Resources.All, OnTenderAllCommand);
            TenderAllBalanceCommand = new CaptionCommand<string>(Resources.Balance, OnTenderAllBalanceCommand);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecuted);
            SetValueCommand = new DelegateCommand<string>(OnSetValue);
            DivideValueCommand = new DelegateCommand<string>(OnDivideValue);
        }

        public event EventHandler TypedValueChanged;
        private void OnTypedValueChanged()
        {
            EventHandler handler = TypedValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public bool ResetAmount { get; set; }

        private string _lastTenderedAmount;
        public string LastTenderedAmount
        {
            get { return _lastTenderedAmount; }
            set { _lastTenderedAmount = value; RaisePropertyChanged(() => LastTenderedAmount); }
        }

        public CaptionCommand<string> TenderAllCommand { get; set; }
        public CaptionCommand<string> TenderAllBalanceCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> SetValueCommand { get; set; }
        public DelegateCommand<string> DivideValueCommand { get; set; }

        private void OnTenderAllBalanceCommand(string obj)
        {
            ResetValues();
            _orderSelectorViewModel.ClearSelection();
            var remaining = _paymentEditor.GetRemainingAmount();
            var accountBalance = _accountBalances.GetActiveAccountBalance();
            var paymentDue = (remaining + accountBalance) / _paymentEditor.ExchangeRate;
            _tenderedValueViewModel.PaymentDueAmount = paymentDue.ToString("#,#0.00");
            _tenderedValueViewModel.TenderedAmount = _tenderedValueViewModel.PaymentDueAmount;
            _foreignCurrencyButtonsViewModel.UpdateCurrencyButtons();
        }

        private void OnTenderAllCommand(string obj)
        {
            _tenderedValueViewModel.TenderedAmount = _tenderedValueViewModel.PaymentDueAmount;
            ResetAmount = true;
            OnTypedValueChanged();
        }

        private void OnDivideValue(string obj)
        {
            decimal tenderedValue = _tenderedValueViewModel.GetTenderedValue();
            ResetAmount = true;
            string dc = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            obj = obj.Replace(",", dc);
            obj = obj.Replace(".", dc);

            decimal value = Convert.ToDecimal(obj);
            var remainingTicketAmount = _tenderedValueViewModel.GetPaymentDueValue() / _paymentEditor.ExchangeRate;

            if (value > 0)
            {
                var amount = remainingTicketAmount / value;
                if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                _tenderedValueViewModel.TenderedAmount = amount.ToString("#,#0.00");
            }
            else
            {
                value = tenderedValue;
                if (value > 0)
                {
                    var amount = remainingTicketAmount / value;
                    if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                    _tenderedValueViewModel.TenderedAmount = (amount).ToString("#,#0.00");
                }
            }
            OnTypedValueChanged();
        }

        private void OnSetValue(string obj)
        {
            ResetAmount = true;
            if (string.IsNullOrEmpty(obj))
            {
                _orderSelectorViewModel.ClearSelection();
                ResetValues();
                return;
            }

            var value = Convert.ToDecimal(obj);
            if (string.IsNullOrEmpty(_tenderedValueViewModel.TenderedAmount))
                _tenderedValueViewModel.TenderedAmount = "0";
            var tenderedValue = Convert.ToDecimal(_tenderedValueViewModel.TenderedAmount.Replace(
                CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, ""));
            tenderedValue += value;
            _tenderedValueViewModel.TenderedAmount = tenderedValue.ToString("#,#0.00");
            OnTypedValueChanged();
        }

        private void OnTypeValueExecuted(string obj)
        {
            if (ResetAmount) _tenderedValueViewModel.TenderedAmount = "";
            ResetAmount = false;
            _tenderedValueViewModel.TenderedAmount = Helpers.AddTypedValue(_tenderedValueViewModel.TenderedAmount, obj, "#,#0.");
            OnTypedValueChanged();
        }

        public void ResetValues()
        {
            _paymentEditor.UpdateCalculations();

            if (_tenderedValueViewModel.GetPaymentDueValue() <= 0)
                _tenderedValueViewModel.UpdatePaymentAmount(0);

            _paymentTotals.ResetCache();
            _paymentTotals.Refresh();
            _accountBalances.Refresh();
            _tenderedValueViewModel.TenderedAmount = "";
        }

    }
}
