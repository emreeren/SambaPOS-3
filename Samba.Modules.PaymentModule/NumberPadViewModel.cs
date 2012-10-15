using System;
using System.ComponentModel.Composition;
using System.Globalization;
using Microsoft.Practices.Prism.Commands;
using Samba.Infrastructure.Settings;
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
            ForeignCurrencyButtonsViewModel foreignCurrencyButtonsViewModel, TicketTotalsViewModel paymentTotals)
        {
            _paymentEditor = paymentEditor;
            _tenderedValueViewModel = tenderedValueViewModel;
            _orderSelectorViewModel = orderSelectorViewModel;
            _accountBalances = accountBalances;
            _foreignCurrencyButtonsViewModel = foreignCurrencyButtonsViewModel;
            _paymentTotals = paymentTotals;

            TenderAllCommand = new CaptionCommand<string>(Resources.All, OnTenderAllCommand);
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

        private bool _balanceMode;
        public bool BalanceMode
        {
            get { return _balanceMode; }
            set
            {
                _balanceMode = value;
                RaisePropertyChanged(() => TenderAllCaption);
            }
        }

        public string TenderAllCaption { get { return BalanceMode ? Resources.Balance : Resources.All; } }

        public CaptionCommand<string> TenderAllCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> SetValueCommand { get; set; }
        public DelegateCommand<string> DivideValueCommand { get; set; }

        private void OnTenderAllCommand(string obj)
        {
            if (BalanceMode)
                TenderAllBalance();
            else TenderAll();
        }

        private void TenderAllBalance()
        {
            ResetValues();
            _paymentEditor.AccountMode = true;
            _orderSelectorViewModel.ClearSelection();
            var paymentDue = _paymentEditor.GetRemainingAmount() / _paymentEditor.ExchangeRate;
            _tenderedValueViewModel.PaymentDueAmount = paymentDue.ToString(LocalSettings.DefaultCurrencyFormat);
            _tenderedValueViewModel.TenderedAmount = _tenderedValueViewModel.PaymentDueAmount;
            _foreignCurrencyButtonsViewModel.UpdateCurrencyButtons();
            ResetAmount = true;
            BalanceMode = false;
        }

        private void TenderAll()
        {
            _paymentEditor.AccountMode = false;
            if (_tenderedValueViewModel.GetTenderedValue() > _paymentEditor.GetRemainingAmount())
                _tenderedValueViewModel.UpdatePaymentAmount(0);
            _tenderedValueViewModel.TenderedAmount = _tenderedValueViewModel.PaymentDueAmount;
            ResetAmount = true;
            OnTypedValueChanged();
            BalanceMode = _accountBalances.GetActiveAccountBalance() > 0;
        }

        private void OnDivideValue(string obj)
        {
            var tenderedValue = _tenderedValueViewModel.GetTenderedValue();
            ResetAmount = true;
            var dc = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            obj = obj.Replace(",", dc);
            obj = obj.Replace(".", dc);

            var value = Convert.ToDecimal(obj);
            var remainingTicketAmount = _tenderedValueViewModel.GetPaymentDueValue() / _paymentEditor.ExchangeRate;

            if (value > 0)
            {
                var amount = remainingTicketAmount / value;
                if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                _tenderedValueViewModel.TenderedAmount = amount.ToString(LocalSettings.DefaultCurrencyFormat);
            }
            else
            {
                value = tenderedValue;
                if (value > 0)
                {
                    var amount = remainingTicketAmount / value;
                    if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                    _tenderedValueViewModel.TenderedAmount = (amount).ToString(LocalSettings.DefaultCurrencyFormat);
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
                _tenderedValueViewModel.UpdatePaymentAmount(0);
                ResetValues();
                return;
            }

            var value = Convert.ToDecimal(obj);
            if (string.IsNullOrEmpty(_tenderedValueViewModel.TenderedAmount))
                _tenderedValueViewModel.TenderedAmount = "0";
            var tenderedValue = Convert.ToDecimal(_tenderedValueViewModel.TenderedAmount.Replace(
                CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, ""));
            tenderedValue += value;
            _tenderedValueViewModel.TenderedAmount = tenderedValue.ToString(LocalSettings.DefaultCurrencyFormat);
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
            RaisePropertyChanged(() => TenderAllCaption);
        }

    }
}
