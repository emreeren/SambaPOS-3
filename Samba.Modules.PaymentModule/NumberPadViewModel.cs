using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;

namespace Samba.Modules.PaymentModule
{
    public class NumberPadViewModel : ObservableObject
    {
        private decimal _exchangeRate;
        private string _paymentDueAmount;

        public NumberPadViewModel()
        {
            _exchangeRate = 1;
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

        public event EventHandler ResetValue;
        private void OnResetValue()
        {
            EventHandler handler = ResetValue;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler PaymentDueChanged;
        public void OnPaymentDueChanged()
        {
            EventHandler handler = PaymentDueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public bool ResetAmount { get; set; }
        public string TenderedAmount { get; set; }

        public string PaymentDueAmount
        {
            get { return _paymentDueAmount; }
            set
            {
                if (_paymentDueAmount != value)
                {
                    _paymentDueAmount = value;
                    OnPaymentDueChanged();
                }
            }
        }

        private string _lastTenderedAmount;
        public string LastTenderedAmount
        {
            get { return _lastTenderedAmount; }
            set { _lastTenderedAmount = value; RaisePropertyChanged(() => LastTenderedAmount); }
        }

        public CaptionCommand<string> TenderAllCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> SetValueCommand { get; set; }
        public DelegateCommand<string> DivideValueCommand { get; set; }

        private decimal GetTenderedValue()
        {
            decimal result;
            decimal.TryParse(TenderedAmount, out result);
            return decimal.Round(result * _exchangeRate, 2);
        }

        private decimal GetPaymentDueValue()
        {
            decimal result;
            decimal.TryParse(PaymentDueAmount, out result);
            return decimal.Round(result * _exchangeRate, 2);
        }

        private void OnTenderAllCommand(string obj)
        {
            TenderedAmount = PaymentDueAmount;
            ResetAmount = true;
            OnTypedValueChanged();
        }

        private void OnDivideValue(string obj)
        {
            decimal tenderedValue = GetTenderedValue();
            ResetAmount = true;
            string dc = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            obj = obj.Replace(",", dc);
            obj = obj.Replace(".", dc);

            decimal value = Convert.ToDecimal(obj);
            var remainingTicketAmount = GetPaymentDueValue() / _exchangeRate;

            if (value > 0)
            {
                var amount = remainingTicketAmount / value;
                if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                TenderedAmount = amount.ToString("#,#0.00");
            }
            else
            {
                value = tenderedValue;
                if (value > 0)
                {
                    var amount = remainingTicketAmount / value;
                    if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                    TenderedAmount = (amount).ToString("#,#0.00");
                }
            }
            OnTypedValueChanged();
        }

        private void OnSetValue(string obj)
        {
            ResetAmount = true;
            if (string.IsNullOrEmpty(obj))
            {
                TenderedAmount = "";
                PaymentDueAmount = "";
                OnResetValue();
                return;
            }

            var value = Convert.ToDecimal(obj);
            if (string.IsNullOrEmpty(TenderedAmount))
                TenderedAmount = "0";
            var tenderedValue = Convert.ToDecimal(TenderedAmount.Replace(
                CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, ""));
            tenderedValue += value;
            TenderedAmount = tenderedValue.ToString("#,#0.00");
            OnTypedValueChanged();
        }

        private void OnTypeValueExecuted(string obj)
        {
            if (ResetAmount) TenderedAmount = "";
            ResetAmount = false;
            TenderedAmount = Helpers.AddTypedValue(TenderedAmount, obj, "#,#0.");
            OnTypedValueChanged();
        }

        public void UpdateExchangeRate(decimal exchangeRate)
        {
            _exchangeRate = exchangeRate;
        }
    }
}
